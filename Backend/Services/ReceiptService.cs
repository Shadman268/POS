using AutoMapper;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IReceiptRepository _receiptRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public ReceiptService(
            IReceiptRepository receiptRepository,
            IMapper mapper,
            AppDbContext context,
            ITenantContext tenantContext)
        {
            _receiptRepository = receiptRepository;
            _mapper = mapper;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext;
        }

        public async Task<Receipt> CreateReceiptAsync(ReceiptDto receiptDto)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = await _context.Tenants.FirstAsync(t => t.Id == tenantId);

            var receiptItems = new List<ReceiptItem>();

            foreach (var item in receiptDto.Items)
            {
                var line = await BuildReceiptLineAsync(tenant, item);
                receiptItems.Add(line);
            }

            var receipt = new Receipt
            {
                TenantId = tenantId,
                CustomerName = receiptDto.CustomerName,
                Total = receiptDto.Total,
                DiscountUnit = receiptDto.DiscountUnit,
                DiscountValue = receiptDto.DiscountValue,
                PriceAfterDiscount = receiptDto.PriceAfterDiscount,
                CashReceived = receiptDto.CashReceived,
                ChangeAmount = receiptDto.ChangeAmount,
                CreatedAt = DateTime.Now,
                Items = receiptItems
            };

            await _context.Receipts.AddAsync(receipt);
            await _context.SaveChangesAsync();

            foreach (var item in receiptItems)
            {
                await DeductStockAsync(tenant, item, receipt.Id);
            }

            await _context.SaveChangesAsync();
            return receipt;
        }

        public async Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync()
        {
            var receipts = await _receiptRepository.GetAllReceiptsAsync();
            return _mapper.Map<IEnumerable<ReceiptDto>>(receipts);
        }

        public async Task<ReceiptDto?> GetReceiptByIdAsync(int id)
        {
            var receipt = await _receiptRepository.GetReceiptByIdAsync(id);
            return receipt != null ? _mapper.Map<ReceiptDto>(receipt) : null;
        }

        private async Task<ReceiptItem> BuildReceiptLineAsync(Tenant tenant, ReceiptItemDto item)
        {
            Medicine? medicine;
            TenantMedicine? tenantMedicine;

            if (PosCatalogConstants.IsCatalogMedicinePosId(item.ProductId))
            {
                var medicineId = PosCatalogConstants.ToMedicineId(item.ProductId);
                medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId && m.IsActive)
                    ?? throw new InvalidOperationException($"Medicine with id {medicineId} not found.");
                tenantMedicine = await _context.TenantMedicines
                    .FirstOrDefaultAsync(tm => tm.TenantId == tenant.Id && tm.MedicineId == medicineId);
            }
            else
            {
                tenantMedicine = await _context.TenantMedicines
                    .Include(tm => tm.Medicine)
                    .FirstOrDefaultAsync(tm => tm.TenantId == tenant.Id && tm.Id == item.ProductId)
                    ?? throw new InvalidOperationException($"Tenant medicine with id {item.ProductId} not found.");
                medicine = tenantMedicine.Medicine;
            }

            var hasPrice = tenantMedicine?.SellingPrice is > 0;
            decimal salePrice;

            if (hasPrice)
            {
                salePrice = tenantMedicine!.SellingPrice!.Value;
            }
            else if (item.Price > 0)
            {
                salePrice = item.Price;
                tenantMedicine = await UpsertTenantMedicinePriceAsync(tenant, medicine, item.Price);
            }
            else if (tenant.PromptPriceWhenUnset)
            {
                throw new InvalidOperationException(
                    $"Price is not set for '{medicine.Name}'. Cashier must provide a price before checkout.");
            }
            else
            {
                throw new InvalidOperationException($"Price is required for '{medicine.Name}'.");
            }

            if (tenantMedicine != null && PosCatalogService.ShouldTrackStock(tenant, tenantMedicine))
            {
                await ValidateStockBeforeSaleAsync(tenant, tenantMedicine, item.Quantity, item.MedicineBatchId);
            }

            MedicineBatch? batch = null;
            if (item.MedicineBatchId.HasValue)
            {
                batch = await _context.MedicineBatches
                    .FirstOrDefaultAsync(b => b.Id == item.MedicineBatchId && b.TenantMedicineId == tenantMedicine!.Id);
            }
            else if (tenant.MaintainStock && tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry && tenantMedicine?.IsStockTracked == true)
            {
                batch = await SelectFefoBatchAsync(tenantMedicine!.Id, item.Quantity);
            }

            return new ReceiptItem
            {
                LineType = SaleLineType.Medicine,
                MedicineId = medicine.Id,
                TenantMedicineId = tenantMedicine?.Id,
                MedicineBatchId = batch?.Id,
                ProductName = medicine.Name,
                GenericName = medicine.GenericName,
                BatchNumber = batch?.BatchNumber,
                ExpiryDate = batch?.ExpiryDate,
                Quantity = item.Quantity,
                Price = salePrice,
                LineDiscount = Math.Max(0, item.LineDiscount),
                Subtotal = item.Subtotal > 0
                    ? item.Subtotal
                    : Math.Max(0, salePrice * item.Quantity - Math.Max(0, item.LineDiscount))
            };
        }

        private async Task<TenantMedicine> UpsertTenantMedicinePriceAsync(Tenant tenant, Medicine medicine, decimal price)
        {
            var existing = await _context.TenantMedicines
                .FirstOrDefaultAsync(tm => tm.TenantId == tenant.Id && tm.MedicineId == medicine.Id);

            if (existing != null)
            {
                existing.SellingPrice = price;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var tenantMedicine = new TenantMedicine
            {
                TenantId = tenant.Id,
                MedicineId = medicine.Id,
                SellingPrice = price,
                IsStockTracked = tenant.MaintainStock && tenant.InventoryMode != PharmacyInventoryMode.CatalogOnly,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.TenantMedicines.Add(tenantMedicine);
            await _context.SaveChangesAsync();
            return tenantMedicine;
        }

        private async Task ValidateStockBeforeSaleAsync(Tenant tenant, TenantMedicine tenantMedicine, int quantity, int? batchId)
        {
            if (!tenant.MaintainStock || tenant.InventoryMode == PharmacyInventoryMode.CatalogOnly)
            {
                return;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
            {
                if (batchId.HasValue)
                {
                    var batch = await _context.MedicineBatches.FirstOrDefaultAsync(b => b.Id == batchId && b.TenantMedicineId == tenantMedicine.Id);
                    if (batch == null || batch.QuantityOnHand < quantity)
                    {
                        throw new InvalidOperationException("Insufficient batch stock.");
                    }
                    return;
                }

                var total = await _context.MedicineBatches
                    .Where(b => b.TenantMedicineId == tenantMedicine.Id && b.IsActive)
                    .SumAsync(b => b.QuantityOnHand);

                if (total < quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for '{tenantMedicine.Medicine?.Name ?? "medicine"}'.");
                }
                return;
            }

            var stock = await _context.MedicineStocks.FirstOrDefaultAsync(s => s.TenantMedicineId == tenantMedicine.Id);
            if ((stock?.QuantityOnHand ?? 0) < quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for '{tenantMedicine.Medicine?.Name ?? "medicine"}'.");
            }
        }

        private async Task<MedicineBatch?> SelectFefoBatchAsync(int tenantMedicineId, int quantity)
        {
            return await _context.MedicineBatches
                .Where(b => b.TenantMedicineId == tenantMedicineId && b.IsActive && b.QuantityOnHand >= quantity)
                .OrderBy(b => b.ExpiryDate)
                .FirstOrDefaultAsync();
        }

        private async Task DeductStockAsync(Tenant tenant, ReceiptItem item, int receiptId)
        {
            if (item.TenantMedicineId == null || !tenant.MaintainStock || tenant.InventoryMode == PharmacyInventoryMode.CatalogOnly)
            {
                return;
            }

            var tenantMedicine = await _context.TenantMedicines
                .Include(tm => tm.Stock)
                .FirstOrDefaultAsync(tm => tm.Id == item.TenantMedicineId);

            if (tenantMedicine == null || !tenantMedicine.IsStockTracked)
            {
                return;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
            {
                var batch = item.MedicineBatchId.HasValue
                    ? await _context.MedicineBatches.FirstOrDefaultAsync(b => b.Id == item.MedicineBatchId)
                    : await SelectFefoBatchAsync(tenantMedicine.Id, item.Quantity);

                if (batch == null)
                {
                    return;
                }

                batch.QuantityOnHand -= item.Quantity;
                item.MedicineBatchId = batch.Id;
                item.BatchNumber = batch.BatchNumber;
                item.ExpiryDate = batch.ExpiryDate;

                _context.StockMovements.Add(new StockMovement
                {
                    TenantId = tenant.Id,
                    TenantMedicineId = tenantMedicine.Id,
                    MedicineBatchId = batch.Id,
                    MovementType = StockMovementType.Sale,
                    QuantityDelta = -item.Quantity,
                    ReferenceType = "Receipt",
                    ReferenceId = receiptId,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await SyncAggregateStockAsync(tenantMedicine);
                return;
            }

            var stock = tenantMedicine.Stock ?? await EnsureStockRowAsync(tenant.Id, tenantMedicine.Id);
            stock.QuantityOnHand -= item.Quantity;
            stock.UpdatedAtUtc = DateTime.UtcNow;

            _context.StockMovements.Add(new StockMovement
            {
                TenantId = tenant.Id,
                TenantMedicineId = tenantMedicine.Id,
                MovementType = StockMovementType.Sale,
                QuantityDelta = -item.Quantity,
                ReferenceType = "Receipt",
                ReferenceId = receiptId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        private async Task SyncAggregateStockAsync(TenantMedicine tenantMedicine)
        {
            var total = await _context.MedicineBatches
                .Where(b => b.TenantMedicineId == tenantMedicine.Id && b.IsActive)
                .SumAsync(b => b.QuantityOnHand);

            var stock = tenantMedicine.Stock ?? await EnsureStockRowAsync(tenantMedicine.TenantId, tenantMedicine.Id);
            stock.QuantityOnHand = total;
            stock.UpdatedAtUtc = DateTime.UtcNow;
        }

        private async Task<MedicineStock> EnsureStockRowAsync(int tenantId, int tenantMedicineId)
        {
            var stock = await _context.MedicineStocks.FirstOrDefaultAsync(s => s.TenantMedicineId == tenantMedicineId);
            if (stock != null)
            {
                return stock;
            }

            stock = new MedicineStock
            {
                TenantId = tenantId,
                TenantMedicineId = tenantMedicineId,
                QuantityOnHand = 0,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _context.MedicineStocks.Add(stock);
            await _context.SaveChangesAsync();
            return stock;
        }
    }
}
