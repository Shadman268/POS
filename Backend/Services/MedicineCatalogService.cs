using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace Backend.Services
{
    public class MedicineCatalogService : IMedicineCatalogService
    {
        private readonly AppDbContext _context;

        public MedicineCatalogService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// One-time CSV import for global medicine catalog.
        /// Expected header: Name,GenericName,Strength,DosageForm,Category,Brand,Unit,Barcode
        /// </summary>
        public async Task<MedicineImportResultDto> ImportFromCsvAsync(Stream csvStream)
        {
            var result = new MedicineImportResultDto();
            using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                result.Errors.Add("CSV file is empty.");
                return result;
            }

            var headers = ParseCsvLine(headerLine).Select(h => h.Trim().ToLowerInvariant()).ToList();
            var nameIdx = IndexOf(headers, "name");
            var genericIdx = IndexOf(headers, "genericname", "generic_name", "generic name");
            var strengthIdx = IndexOf(headers, "strength");
            var formIdx = IndexOf(headers, "dosageform", "dosage_form", "form");
            var categoryIdx = IndexOf(headers, "category");
            var brandIdx = IndexOf(headers, "brand");
            var unitIdx = IndexOf(headers, "unit");
            var barcodeIdx = IndexOf(headers, "barcode");

            if (nameIdx < 0)
            {
                result.Errors.Add("CSV must include a Name column.");
                return result;
            }

            var existingByName = await _context.Medicines
                .ToDictionaryAsync(m => NormalizeKey(m.Name, m.GenericName), m => m);

            string? line;
            var lineNo = 1;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNo++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var cols = ParseCsvLine(line);
                if (cols.Count == 0)
                {
                    continue;
                }

                var name = GetCol(cols, nameIdx)?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Skipped++;
                    continue;
                }

                var genericName = GetCol(cols, genericIdx)?.Trim();
                var key = NormalizeKey(name, genericName);

                if (existingByName.TryGetValue(key, out var existing))
                {
                    existing.GenericName = genericName ?? existing.GenericName;
                    existing.Strength = GetCol(cols, strengthIdx)?.Trim() ?? existing.Strength;
                    existing.DosageForm = GetCol(cols, formIdx)?.Trim() ?? existing.DosageForm;
                    existing.Category = GetCol(cols, categoryIdx)?.Trim() ?? existing.Category;
                    existing.Brand = GetCol(cols, brandIdx)?.Trim() ?? existing.Brand;
                    existing.Unit = GetCol(cols, unitIdx)?.Trim() ?? existing.Unit;
                    existing.Barcode = GetCol(cols, barcodeIdx)?.Trim() ?? existing.Barcode;
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                    existing.IsActive = true;
                    result.Updated++;
                    continue;
                }

                var medicine = new Medicine
                {
                    Name = name,
                    GenericName = genericName,
                    Strength = GetCol(cols, strengthIdx)?.Trim(),
                    DosageForm = GetCol(cols, formIdx)?.Trim(),
                    Category = GetCol(cols, categoryIdx)?.Trim() ?? "Medicine",
                    Brand = GetCol(cols, brandIdx)?.Trim() ?? "General",
                    Unit = GetCol(cols, unitIdx)?.Trim() ?? "Tablet",
                    Barcode = GetCol(cols, barcodeIdx)?.Trim(),
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _context.Medicines.Add(medicine);
                existingByName[key] = medicine;
                result.Inserted++;

                if ((result.Inserted + result.Updated) % 500 == 0)
                {
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private static int IndexOf(List<string> headers, params string[] names)
        {
            foreach (var name in names)
            {
                var idx = headers.IndexOf(name);
                if (idx >= 0)
                {
                    return idx;
                }
            }
            return -1;
        }

        private static string? GetCol(List<string> cols, int index)
        {
            return index >= 0 && index < cols.Count ? cols[index] : null;
        }

        private static string NormalizeKey(string name, string? genericName)
        {
            return $"{name.Trim().ToLowerInvariant()}|{(genericName ?? string.Empty).Trim().ToLowerInvariant()}";
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        public async Task<PagedMedicineResultDto> GetMedicinesAsync(int page, int pageSize, string? search = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Medicines.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)) ||
                    (m.Barcode != null && m.Barcode.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MedicineDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    GenericName = m.GenericName,
                    Strength = m.Strength,
                    DosageForm = m.DosageForm,
                    Category = m.Category,
                    Brand = m.Brand,
                    Unit = m.Unit,
                    Barcode = m.Barcode,
                    IsActive = m.IsActive,
                    CreatedAtUtc = m.CreatedAtUtc,
                    UpdatedAtUtc = m.UpdatedAtUtc
                })
                .ToListAsync();

            return new PagedMedicineResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
