using Backend.Constants;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class PosCatalogService : IPosCatalogService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public PosCatalogService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IEnumerable<ProductDto>> GetPosCatalogAsync(string? search = null, string? category = null, string? brand = null)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = await _context.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenantId);

            var query = _context.Medicines
                .AsNoTracking()
                .Where(m => m.IsActive);

            var hasSearch = !string.IsNullOrWhiteSpace(search);

            if (hasSearch)
            {
                var term = search!.Trim().ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)) ||
                    (m.Barcode != null && m.Barcode.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All Category")
            {
                query = query.Where(m => m.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(brand) && brand != "All Brand")
            {
                query = query.Where(m => m.Brand == brand);
            }

            List<Medicine> medicines;
            if (hasSearch)
            {
                var term = search!.Trim().ToLower();
                var candidates = await query.Take(250).ToListAsync();
                medicines = candidates
                    .Select(m => (Medicine: m, Score: ScoreMedicineMatch(m, term)))
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Medicine.Name)
                    .Take(25)
                    .Select(x => x.Medicine)
                    .ToList();
            }
            else
            {
                medicines = await query
                    .OrderBy(m => m.Name)
                    .Take(500)
                    .ToListAsync();
            }

            var medicineIds = medicines.Select(m => m.Id).ToList();
            var tenantMedicines = await _context.TenantMedicines
                .AsNoTracking()
                .Include(tm => tm.Stock)
                .Include(tm => tm.Batches.Where(b => b.IsActive))
                .Where(tm => tm.TenantId == tenantId && medicineIds.Contains(tm.MedicineId))
                .ToListAsync();

            var tenantMap = tenantMedicines.ToDictionary(tm => tm.MedicineId);

            return medicines.Select(m => MapToProductDto(m, tenantMap.GetValueOrDefault(m.Id), tenant.InventoryMode));
        }

        private static int ScoreMedicineMatch(Medicine medicine, string term)
        {
            var name = medicine.Name.ToLowerInvariant();
            var generic = medicine.GenericName?.ToLowerInvariant() ?? string.Empty;
            var barcode = medicine.Barcode?.ToLowerInvariant() ?? string.Empty;

            if (name == term)
            {
                return 1000;
            }

            if (barcode == term)
            {
                return 950;
            }

            if (generic == term)
            {
                return 900;
            }

            if (name.StartsWith(term, StringComparison.Ordinal))
            {
                return 800;
            }

            if (generic.StartsWith(term, StringComparison.Ordinal))
            {
                return 700;
            }

            if (barcode.StartsWith(term, StringComparison.Ordinal))
            {
                return 650;
            }

            if (name.Contains(term, StringComparison.Ordinal))
            {
                return 600;
            }

            if (generic.Contains(term, StringComparison.Ordinal))
            {
                return 500;
            }

            if (barcode.Contains(term, StringComparison.Ordinal))
            {
                return 400;
            }

            return 0;
        }

        public async Task<ResolvePosItemResponse> ResolvePosItemAsync(ResolvePosItemRequest request)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = await _context.Tenants.FirstAsync(t => t.Id == tenantId);

            var (medicine, tenantMedicine) = await ResolveEntitiesAsync(tenantId, request.PosItemId);

            if (medicine == null)
            {
                return new ResolvePosItemResponse
                {
                    Success = false,
                    Message = "Medicine not found."
                };
            }

            var hasPrice = tenantMedicine?.SellingPrice is > 0;

            if (!hasPrice)
            {
                if (tenant.PromptPriceWhenUnset && (request.Price == null || request.Price <= 0))
                {
                    return new ResolvePosItemResponse
                    {
                        Success = false,
                        RequiresPrice = true,
                        Message = "Selling price is not set. Please enter a price.",
                        Item = MapToProductDto(medicine, tenantMedicine, tenant.InventoryMode)
                    };
                }

                if (request.Price is > 0)
                {
                    tenantMedicine = await EnsureTenantMedicineAsync(tenantId, medicine, tenant, request.Price.Value);
                    hasPrice = true;
                }
            }

            if (!hasPrice)
            {
                return new ResolvePosItemResponse
                {
                    Success = false,
                    RequiresPrice = true,
                    Message = "Selling price is required.",
                    Item = MapToProductDto(medicine, tenantMedicine, tenant.InventoryMode)
                };
            }

            if (tenantMedicine != null && tenantMedicine.IsStockTracked)
            {
                var stockError = await ValidateStockAsync(tenant, tenantMedicine, request.Quantity, request.MedicineBatchId);
                if (stockError != null)
                {
                    return new ResolvePosItemResponse
                    {
                        Success = false,
                        Message = stockError,
                        Item = MapToProductDto(medicine, tenantMedicine, tenant.InventoryMode)
                    };
                }
            }

            return new ResolvePosItemResponse
            {
                Success = true,
                Item = MapToProductDto(medicine, tenantMedicine, tenant.InventoryMode)
            };
        }

        internal static ProductDto MapToProductDto(Medicine medicine, TenantMedicine? tenantMedicine, PharmacyInventoryMode mode)
        {
            var hasPrice = tenantMedicine?.SellingPrice is > 0;
            var isStockTracked = tenantMedicine?.IsStockTracked == true && mode != PharmacyInventoryMode.CatalogOnly;

            var stockQty = 0;
            if (isStockTracked && tenantMedicine != null)
            {
                stockQty = mode == PharmacyInventoryMode.BatchExpiry
                    ? tenantMedicine.Batches.Where(b => b.IsActive).Sum(b => b.QuantityOnHand)
                    : tenantMedicine.Stock?.QuantityOnHand ?? 0;
            }
            else if (mode == PharmacyInventoryMode.CatalogOnly)
            {
                stockQty = 999;
            }

            var id = tenantMedicine?.Id ?? PosCatalogConstants.ToPosItemId(medicine.Id);

            return new ProductDto
            {
                Id = id,
                MedicineId = medicine.Id,
                TenantMedicineId = tenantMedicine?.Id,
                ProductName = medicine.Name,
                GenericName = medicine.GenericName,
                Price = tenantMedicine?.SellingPrice ?? 0,
                RequiresPrice = !hasPrice,
                HasTenantPrice = hasPrice,
                Category = medicine.Category,
                Brand = medicine.Brand,
                StockQuantity = stockQty,
                Unit = medicine.Unit,
                IsStockTracked = isStockTracked
            };
        }

        private async Task<(Medicine? medicine, TenantMedicine? tenantMedicine)> ResolveEntitiesAsync(int tenantId, int posItemId)
        {
            if (PosCatalogConstants.IsCatalogMedicinePosId(posItemId))
            {
                var medicineId = PosCatalogConstants.ToMedicineId(posItemId);
                var medicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Id == medicineId && m.IsActive);
                return (medicine, null);
            }

            var tenantMedicine = await _context.TenantMedicines
                .Include(tm => tm.Medicine)
                .Include(tm => tm.Stock)
                .Include(tm => tm.Batches.Where(b => b.IsActive))
                .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.Id == posItemId);

            return (tenantMedicine?.Medicine, tenantMedicine);
        }

        private async Task<TenantMedicine> EnsureTenantMedicineAsync(int tenantId, Medicine medicine, Tenant tenant, decimal price)
        {
            var existing = await _context.TenantMedicines
                .Include(tm => tm.Stock)
                .Include(tm => tm.Batches)
                .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.MedicineId == medicine.Id);

            if (existing != null)
            {
                existing.SellingPrice = price;
                existing.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var isStockTracked = tenant.InventoryMode != PharmacyInventoryMode.CatalogOnly;
            var tenantMedicine = new TenantMedicine
            {
                TenantId = tenantId,
                MedicineId = medicine.Id,
                SellingPrice = price,
                IsStockTracked = isStockTracked,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.TenantMedicines.Add(tenantMedicine);
            await _context.SaveChangesAsync();
            return tenantMedicine;
        }

        private async Task<string?> ValidateStockAsync(Tenant tenant, TenantMedicine tenantMedicine, int quantity, int? batchId)
        {
            if (tenant.InventoryMode == PharmacyInventoryMode.CatalogOnly)
            {
                return null;
            }

            if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
            {
                if (batchId.HasValue)
                {
                    var batch = await _context.MedicineBatches
                        .FirstOrDefaultAsync(b => b.Id == batchId && b.TenantMedicineId == tenantMedicine.Id && b.IsActive);
                    if (batch == null)
                    {
                        return "Batch not found.";
                    }
                    if (batch.QuantityOnHand < quantity)
                    {
                        return $"Insufficient stock in batch {batch.BatchNumber}. Available: {batch.QuantityOnHand}.";
                    }
                    return null;
                }

                var totalBatchStock = await _context.MedicineBatches
                    .Where(b => b.TenantMedicineId == tenantMedicine.Id && b.IsActive)
                    .SumAsync(b => b.QuantityOnHand);

                if (totalBatchStock < quantity)
                {
                    return $"Insufficient stock. Available: {totalBatchStock}.";
                }
                return null;
            }

            var stock = await _context.MedicineStocks
                .FirstOrDefaultAsync(s => s.TenantMedicineId == tenantMedicine.Id);

            var onHand = stock?.QuantityOnHand ?? 0;
            if (onHand < quantity)
            {
                return $"Insufficient stock. Available: {onHand}.";
            }

            return null;
        }
    }
}
