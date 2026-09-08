using AutoMapper;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Backend.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IReceiptRepository _receiptRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly IStockInventoryService _stockInventoryService;

        public ReceiptService(
            IReceiptRepository receiptRepository,
            IMapper mapper,
            AppDbContext context,
            ITenantContext tenantContext,
            IStockInventoryService stockInventoryService)
        {
            _receiptRepository = receiptRepository;
            _mapper = mapper;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext;
            _stockInventoryService = stockInventoryService;
        }

        public async Task<Receipt> CreateReceiptAsync(ReceiptDto receiptDto)
        {
            var tenantId = _tenantContext.TenantId;
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var tenant = await _context.Tenants.FirstAsync(t => t.Id == tenantId);
                var receiptItems = new List<ReceiptItem>();

                foreach (var item in receiptDto.Items)
                {
                    receiptItems.Add(await BuildReceiptLineAsync(tenant, item));
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
                    await _stockInventoryService.DeductForSaleAsync(tenant, item, receipt.Id);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return receipt;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

            MedicineBatch? batch = null;
            if (item.MedicineBatchId.HasValue && tenantMedicine != null)
            {
                batch = await _context.MedicineBatches
                    .FirstOrDefaultAsync(b => b.Id == item.MedicineBatchId && b.TenantMedicineId == tenantMedicine.Id);
            }
            else if (tenant.MaintainStock
                && tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry
                && tenantMedicine?.IsStockTracked == true)
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
            var isStockTracked = tenant.MaintainStock && tenant.InventoryMode != PharmacyInventoryMode.CatalogOnly;
            var existing = await _context.TenantMedicines
                .Include(tm => tm.Stock)
                .FirstOrDefaultAsync(tm => tm.TenantId == tenant.Id && tm.MedicineId == medicine.Id);

            if (existing != null)
            {
                existing.SellingPrice = price;
                existing.IsStockTracked = isStockTracked;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await EnsureStockRowAsync(tenant.Id, existing, isStockTracked);
                return existing;
            }

            var tenantMedicine = new TenantMedicine
            {
                TenantId = tenant.Id,
                MedicineId = medicine.Id,
                SellingPrice = price,
                IsStockTracked = isStockTracked,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.TenantMedicines.Add(tenantMedicine);
            await _context.SaveChangesAsync();
            await EnsureStockRowAsync(tenant.Id, tenantMedicine, isStockTracked);
            return tenantMedicine;
        }

        private async Task EnsureStockRowAsync(int tenantId, TenantMedicine tenantMedicine, bool isStockTracked)
        {
            if (!isStockTracked || tenantMedicine.Stock != null)
            {
                return;
            }

            var stock = await _context.MedicineStocks
                .FirstOrDefaultAsync(s => s.TenantMedicineId == tenantMedicine.Id);

            if (stock != null)
            {
                tenantMedicine.Stock = stock;
                return;
            }

            stock = new MedicineStock
            {
                TenantId = tenantId,
                TenantMedicineId = tenantMedicine.Id,
                QuantityOnHand = 0,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _context.MedicineStocks.Add(stock);
            tenantMedicine.Stock = stock;
        }

        private async Task<MedicineBatch?> SelectFefoBatchAsync(int tenantMedicineId, int quantity)
        {
            return await _context.MedicineBatches
                .Where(b => b.TenantMedicineId == tenantMedicineId && b.IsActive && b.QuantityOnHand >= quantity)
                .OrderBy(b => b.ExpiryDate)
                .FirstOrDefaultAsync();
        }
    }
}
