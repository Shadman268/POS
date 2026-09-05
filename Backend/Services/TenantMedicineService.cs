using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class TenantMedicineService : ITenantMedicineService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TenantMedicineService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<PagedTenantMedicineResultDto> GetTenantMedicinesAsync(int page, int pageSize, string? search = null)
        {
            var tenantId = _tenantContext.TenantId;
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var medicinesQuery = _context.Medicines
                .AsNoTracking()
                .Where(m => m.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                medicinesQuery = medicinesQuery.Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)) ||
                    (m.Barcode != null && m.Barcode.ToLower().Contains(term)));
            }

            var totalCount = await medicinesQuery.CountAsync();

            var medicines = await medicinesQuery
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var medicineIds = medicines.Select(m => m.Id).ToList();

            var tenantMedicines = await _context.TenantMedicines
                .AsNoTracking()
                .Include(tm => tm.Stock)
                .Where(tm => tm.TenantId == tenantId && medicineIds.Contains(tm.MedicineId))
                .ToDictionaryAsync(tm => tm.MedicineId);

            var items = medicines
                .Select(m => MapToDto(m, tenantMedicines.GetValueOrDefault(m.Id)))
                .ToList();

            return new PagedTenantMedicineResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TenantMedicineDto> UpsertTenantMedicineSettingsAsync(int medicineId, UpdateTenantMedicineSettingsDto dto)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = await _context.Tenants.FirstAsync(t => t.Id == tenantId);

            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == medicineId && m.IsActive)
                ?? throw new InvalidOperationException("Medicine not found.");

            var tenantMedicine = await _context.TenantMedicines
                .Include(tm => tm.Stock)
                .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.MedicineId == medicineId);

            var isStockTracked = dto.IsStockTracked && tenant.InventoryMode != PharmacyInventoryMode.CatalogOnly;

            if (tenantMedicine == null)
            {
                tenantMedicine = new TenantMedicine
                {
                    TenantId = tenantId,
                    MedicineId = medicineId,
                    SellingPrice = dto.SellingPrice,
                    CostPrice = dto.CostPrice,
                    IsStockTracked = isStockTracked,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _context.TenantMedicines.Add(tenantMedicine);
                await _context.SaveChangesAsync();
            }
            else
            {
                tenantMedicine.SellingPrice = dto.SellingPrice;
                tenantMedicine.CostPrice = dto.CostPrice;
                tenantMedicine.IsStockTracked = isStockTracked;
                tenantMedicine.UpdatedAtUtc = DateTime.UtcNow;
            }

            if (isStockTracked)
            {
                var stock = tenantMedicine.Stock;
                if (stock == null)
                {
                    stock = new MedicineStock
                    {
                        TenantId = tenantId,
                        TenantMedicineId = tenantMedicine.Id,
                        QuantityOnHand = Math.Max(0, dto.StockQuantity),
                        UpdatedAtUtc = DateTime.UtcNow
                    };
                    _context.MedicineStocks.Add(stock);
                    tenantMedicine.Stock = stock;
                }
                else
                {
                    stock.QuantityOnHand = Math.Max(0, dto.StockQuantity);
                    stock.UpdatedAtUtc = DateTime.UtcNow;
                }

                if (tenant.InventoryMode == PharmacyInventoryMode.BatchExpiry)
                {
                    var batch = await _context.MedicineBatches
                        .Where(b => b.TenantMedicineId == tenantMedicine.Id && b.IsActive)
                        .OrderBy(b => b.ExpiryDate)
                        .FirstOrDefaultAsync();

                    if (batch == null && dto.StockQuantity > 0)
                    {
                        _context.MedicineBatches.Add(new MedicineBatch
                        {
                            TenantId = tenantId,
                            TenantMedicineId = tenantMedicine.Id,
                            BatchNumber = $"B-{DateTime.UtcNow:yyyyMMddHHmmss}",
                            ExpiryDate = DateTime.UtcNow.Date.AddYears(1),
                            QuantityOnHand = dto.StockQuantity,
                            CostPrice = dto.CostPrice,
                            ReceivedAtUtc = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                    else if (batch != null)
                    {
                        batch.QuantityOnHand = Math.Max(0, dto.StockQuantity);
                    }
                }
            }
            else if (tenantMedicine.Stock != null)
            {
                tenantMedicine.Stock.QuantityOnHand = 0;
                tenantMedicine.Stock.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (tenantMedicine.Stock == null && isStockTracked)
            {
                await _context.Entry(tenantMedicine).Reference(tm => tm.Stock).LoadAsync();
            }

            return MapToDto(medicine, tenantMedicine);
        }

        public async Task<TenantMedicineDto> CreateTenantMedicineAsync(CreateTenantMedicineDto dto)
        {
            var tenantId = _tenantContext.TenantId;
            var tenant = await _context.Tenants.FirstAsync(t => t.Id == tenantId);

            var normalizedGeneric = dto.GenericName?.Trim() ?? string.Empty;
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m =>
                    m.Name == dto.Name.Trim() &&
                    (m.GenericName ?? string.Empty) == normalizedGeneric);

            if (medicine == null)
            {
                medicine = new Medicine
                {
                    Name = dto.Name.Trim(),
                    GenericName = string.IsNullOrWhiteSpace(dto.GenericName) ? null : dto.GenericName.Trim(),
                    Strength = dto.Strength?.Trim(),
                    DosageForm = dto.DosageForm?.Trim(),
                    Category = string.IsNullOrWhiteSpace(dto.Category) ? "Medicine" : dto.Category.Trim(),
                    Brand = string.IsNullOrWhiteSpace(dto.Brand) ? "General" : dto.Brand.Trim(),
                    Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "Tablet" : dto.Unit.Trim(),
                    Barcode = dto.Barcode?.Trim(),
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _context.Medicines.Add(medicine);
                await _context.SaveChangesAsync();
            }

            return await UpsertTenantMedicineSettingsAsync(medicine.Id, new UpdateTenantMedicineSettingsDto
            {
                SellingPrice = dto.SellingPrice,
                CostPrice = dto.CostPrice,
                IsStockTracked = dto.IsStockTracked,
                StockQuantity = dto.InitialStock
            });
        }

        public async Task DeleteTenantMedicineAsync(int id)
        {
            var tenantId = _tenantContext.TenantId;

            var tenantMedicine = await _context.TenantMedicines
                .FirstOrDefaultAsync(tm => tm.Id == id && tm.TenantId == tenantId)
                ?? throw new InvalidOperationException("Product not found.");

            await RemoveTenantMedicineAsync(tenantMedicine);
        }

        public async Task ResetTenantMedicineByMedicineIdAsync(int medicineId)
        {
            var tenantId = _tenantContext.TenantId;

            var tenantMedicine = await _context.TenantMedicines
                .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.MedicineId == medicineId);

            if (tenantMedicine == null)
            {
                return;
            }

            await RemoveTenantMedicineAsync(tenantMedicine);
        }

        private async Task RemoveTenantMedicineAsync(TenantMedicine tenantMedicine)
        {
            var id = tenantMedicine.Id;

            await _context.StockMovements
                .Where(sm => sm.TenantMedicineId == id)
                .ExecuteDeleteAsync();

            await _context.MedicineBatches
                .Where(b => b.TenantMedicineId == id)
                .ExecuteDeleteAsync();

            await _context.MedicineStocks
                .Where(s => s.TenantMedicineId == id)
                .ExecuteDeleteAsync();

            await _context.ReceiptItems
                .Where(ri => ri.TenantMedicineId == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(ri => ri.TenantMedicineId, (int?)null));

            _context.TenantMedicines.Remove(tenantMedicine);
            await _context.SaveChangesAsync();
        }

        private static TenantMedicineDto MapToDto(Medicine medicine, TenantMedicine? tenantMedicine)
        {
            return new TenantMedicineDto
            {
                Id = tenantMedicine?.Id ?? 0,
                MedicineId = medicine.Id,
                Name = medicine.Name,
                GenericName = medicine.GenericName,
                Strength = medicine.Strength,
                DosageForm = medicine.DosageForm,
                Category = medicine.Category,
                Brand = medicine.Brand,
                Unit = medicine.Unit,
                Barcode = medicine.Barcode,
                SellingPrice = tenantMedicine?.SellingPrice,
                CostPrice = tenantMedicine?.CostPrice,
                IsStockTracked = tenantMedicine?.IsStockTracked ?? false,
                StockQuantity = tenantMedicine?.Stock?.QuantityOnHand ?? 0,
                LocalSku = tenantMedicine?.LocalSku
            };
        }
    }
}
