using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class TenantCatalogService : ITenantCatalogService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TenantCatalogService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<TenantCatalogOptionsDto> GetOptionsAsync()
        {
            var tenantId = _tenantContext.TenantId;

            var savedOptions = await _context.TenantCatalogOptions
                .AsNoTracking()
                .Where(o => o.TenantId == tenantId)
                .ToListAsync();

            var medicineIds = await _context.TenantMedicines
                .AsNoTracking()
                .Where(tm => tm.TenantId == tenantId)
                .Select(tm => tm.MedicineId)
                .ToListAsync();

            var medicines = medicineIds.Count == 0
                ? new List<Medicine>()
                : await _context.Medicines
                    .AsNoTracking()
                    .Where(m => medicineIds.Contains(m.Id))
                    .ToListAsync();

            return new TenantCatalogOptionsDto
            {
                DosageForms = MergeNames(
                    savedOptions.Where(o => o.OptionType == CatalogOptionType.DosageForm).Select(o => o.Name),
                    medicines.Where(m => !string.IsNullOrWhiteSpace(m.DosageForm)).Select(m => m.DosageForm!)),
                Brands = MergeNames(
                    savedOptions.Where(o => o.OptionType == CatalogOptionType.Brand).Select(o => o.Name),
                    medicines.Where(m => !string.IsNullOrWhiteSpace(m.Brand)).Select(m => m.Brand)),
                Units = MergeNames(
                    savedOptions.Where(o => o.OptionType == CatalogOptionType.Unit).Select(o => o.Name),
                    medicines.Where(m => !string.IsNullOrWhiteSpace(m.Unit)).Select(m => m.Unit))
            };
        }

        public async Task<TenantCatalogOptionsDto> AddOptionAsync(CatalogOptionType type, string name)
        {
            var trimmed = name.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                throw new InvalidOperationException("Name is required.");
            }

            await SaveOptionIfMissingAsync(type, trimmed);
            await _context.SaveChangesAsync();
            return await GetOptionsAsync();
        }

        public async Task EnsureOptionsAsync(string? dosageForm, string? brand, string? unit)
        {
            var changed = false;

            if (!string.IsNullOrWhiteSpace(dosageForm))
            {
                changed |= await SaveOptionIfMissingAsync(CatalogOptionType.DosageForm, dosageForm.Trim());
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                changed |= await SaveOptionIfMissingAsync(CatalogOptionType.Brand, brand.Trim());
            }

            if (!string.IsNullOrWhiteSpace(unit))
            {
                changed |= await SaveOptionIfMissingAsync(CatalogOptionType.Unit, unit.Trim());
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task<bool> SaveOptionIfMissingAsync(CatalogOptionType type, string name)
        {
            var tenantId = _tenantContext.TenantId;
            var normalized = name.Trim();
            var lower = normalized.ToLowerInvariant();

            var exists = await _context.TenantCatalogOptions.AnyAsync(o =>
                o.TenantId == tenantId &&
                o.OptionType == type &&
                o.Name.ToLower() == lower);

            if (exists)
            {
                return false;
            }

            _context.TenantCatalogOptions.Add(new TenantCatalogOption
            {
                TenantId = tenantId,
                OptionType = type,
                Name = normalized,
                CreatedAtUtc = DateTime.UtcNow
            });

            return true;
        }

        private static List<string> MergeNames(IEnumerable<string> primary, IEnumerable<string> secondary)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();

            foreach (var name in primary.Concat(secondary))
            {
                var trimmed = name.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || !seen.Add(trimmed))
                {
                    continue;
                }

                result.Add(trimmed);
            }

            return result.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
