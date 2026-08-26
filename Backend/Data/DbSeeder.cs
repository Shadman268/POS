using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await SeedTenantsAsync(context);
            await SeedGlobalMedicinesAsync(context);
            await SeedDemoTenantDataAsync(context);
        }

        private static async Task SeedTenantsAsync(AppDbContext context)
        {
            var tenants = new[]
            {
                new Tenant { ShopCode = "demo", Name = "Demo Pharmacy (Batch)", InventoryMode = PharmacyInventoryMode.BatchExpiry, PromptPriceWhenUnset = true },
                new Tenant { ShopCode = "demo-catalog", Name = "Demo Catalog Only", InventoryMode = PharmacyInventoryMode.CatalogOnly, PromptPriceWhenUnset = true },
                new Tenant { ShopCode = "demo-stock", Name = "Demo Stock Tracked", InventoryMode = PharmacyInventoryMode.StockTracked, PromptPriceWhenUnset = true }
            };

            foreach (var tenant in tenants)
            {
                if (!await context.Tenants.AnyAsync(t => t.ShopCode == tenant.ShopCode))
                {
                    tenant.CreatedAtUtc = DateTime.UtcNow;
                    context.Tenants.Add(tenant);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedGlobalMedicinesAsync(AppDbContext context)
        {
            if (await context.Medicines.AnyAsync())
            {
                return;
            }

            var medicines = new[]
            {
                new Medicine { Name = "Napa Extra 500mg", GenericName = "Paracetamol + Caffeine", Category = "Medicine", Brand = "Beximco", Unit = "Tablet" },
                new Medicine { Name = "Seclo 20mg", GenericName = "Omeprazole", Category = "Medicine", Brand = "Square", Unit = "Tablet" },
                new Medicine { Name = "Amodis 400mg", GenericName = "Albendazole", Category = "Medicine", Brand = "General", Unit = "Tablet" },
                new Medicine { Name = "Fexo 120mg", GenericName = "Fexofenadine", Category = "Medicine", Brand = "Incepta", Unit = "Tablet" },
                new Medicine { Name = "Insulin Mixtard", GenericName = "Insulin Human", Category = "Medicine", Brand = "Novo Nordisk", Unit = "Vial" },
                new Medicine { Name = "ORSaline Sachet", GenericName = "Oral Rehydration Salts", Category = "Medicine", Brand = "General", Unit = "Sachet" },
                new Medicine { Name = "Vitamin C 500mg", GenericName = "Ascorbic Acid", Category = "Medicine", Brand = "General", Unit = "Tablet" },
                new Medicine { Name = "Digital BP Monitor", GenericName = "Blood Pressure Monitor", Category = "Equipment", Brand = "Omron", Unit = "Unit" },
                // No tenant price — for testing PromptPriceWhenUnset flow
                new Medicine { Name = "Ecosprin 75mg", GenericName = "Aspirin", Category = "Medicine", Brand = "General", Unit = "Tablet" }
            };

            context.Medicines.AddRange(medicines);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoTenantDataAsync(AppDbContext context)
        {
            var medicines = await context.Medicines.ToListAsync();
            var medicineMap = medicines.ToDictionary(m => m.Name, m => m);
            var today = DateTime.Today;

            await SeedBatchTenantAsync(context, "demo", medicineMap, today);
            await SeedCatalogTenantAsync(context, "demo-catalog", medicineMap);
            await SeedStockTenantAsync(context, "demo-stock", medicineMap);
        }

        private static async Task SeedBatchTenantAsync(AppDbContext context, string shopCode, Dictionary<string, Medicine> medicineMap, DateTime today)
        {
            var tenant = await context.Tenants.FirstAsync(t => t.ShopCode == shopCode);
            if (await context.TenantMedicines.AnyAsync(tm => tm.TenantId == tenant.Id))
            {
                return;
            }

            var configs = new (string name, decimal price, int stock, string? batch, int? expiryDays)[]
            {
                ("Napa Extra 500mg", 12, 86, "NX-2291", 12),
                ("Seclo 20mg", 12, 3, "SC-8841", 18),
                ("Amodis 400mg", 15, 60, "AM-1032", 21),
                ("Fexo 120mg", 10, 0, "FX-7710", 27),
                ("Insulin Mixtard", 450, 24, "IM-0092", 29),
                ("ORSaline Sachet", 10, 200, null, null),
                ("Vitamin C 500mg", 20, 150, null, null),
                ("Digital BP Monitor", 1450, 12, null, null)
            };

            foreach (var cfg in configs)
            {
                if (!medicineMap.TryGetValue(cfg.name, out var medicine))
                {
                    continue;
                }

                var tenantMedicine = new TenantMedicine
                {
                    TenantId = tenant.Id,
                    MedicineId = medicine.Id,
                    SellingPrice = cfg.price,
                    IsStockTracked = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                context.TenantMedicines.Add(tenantMedicine);
                await context.SaveChangesAsync();

                var stock = new MedicineStock
                {
                    TenantId = tenant.Id,
                    TenantMedicineId = tenantMedicine.Id,
                    QuantityOnHand = cfg.stock,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                context.MedicineStocks.Add(stock);

                if (!string.IsNullOrEmpty(cfg.batch) && cfg.expiryDays.HasValue)
                {
                    context.MedicineBatches.Add(new MedicineBatch
                    {
                        TenantId = tenant.Id,
                        TenantMedicineId = tenantMedicine.Id,
                        BatchNumber = cfg.batch,
                        ExpiryDate = today.AddDays(cfg.expiryDays.Value),
                        QuantityOnHand = cfg.stock,
                        ReceivedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCatalogTenantAsync(AppDbContext context, string shopCode, Dictionary<string, Medicine> medicineMap)
        {
            var tenant = await context.Tenants.FirstAsync(t => t.ShopCode == shopCode);
            if (await context.TenantMedicines.AnyAsync(tm => tm.TenantId == tenant.Id))
            {
                return;
            }

            // Catalog-only: some medicines have preset prices, some don't (price prompt flow)
            var withPrice = new Dictionary<string, decimal>
            {
                ["Napa Extra 500mg"] = 12,
                ["Seclo 20mg"] = 12,
                ["Amodis 400mg"] = 15
            };

            foreach (var medicine in medicineMap.Values)
            {
                if (!withPrice.TryGetValue(medicine.Name, out var price))
                {
                    continue;
                }

                context.TenantMedicines.Add(new TenantMedicine
                {
                    TenantId = tenant.Id,
                    MedicineId = medicine.Id,
                    SellingPrice = price,
                    IsStockTracked = false,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedStockTenantAsync(AppDbContext context, string shopCode, Dictionary<string, Medicine> medicineMap)
        {
            var tenant = await context.Tenants.FirstAsync(t => t.ShopCode == shopCode);
            if (await context.TenantMedicines.AnyAsync(tm => tm.TenantId == tenant.Id))
            {
                return;
            }

            var configs = new (string name, decimal price, int stock, bool tracked)[]
            {
                ("Napa Extra 500mg", 12, 86, true),
                ("Seclo 20mg", 12, 3, true),
                ("Amodis 400mg", 15, 60, true),
                ("Fexo 120mg", 10, 0, true),
                ("ORSaline Sachet", 10, 200, false),
                ("Ecosprin 75mg", 0, 0, false)
            };

            foreach (var cfg in configs)
            {
                if (!medicineMap.TryGetValue(cfg.name, out var medicine))
                {
                    continue;
                }

                decimal? price = cfg.price > 0 ? cfg.price : null;
                var tenantMedicine = new TenantMedicine
                {
                    TenantId = tenant.Id,
                    MedicineId = medicine.Id,
                    SellingPrice = price,
                    IsStockTracked = cfg.tracked,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                context.TenantMedicines.Add(tenantMedicine);
                await context.SaveChangesAsync();

                if (cfg.tracked)
                {
                    context.MedicineStocks.Add(new MedicineStock
                    {
                        TenantId = tenant.Id,
                        TenantMedicineId = tenantMedicine.Id,
                        QuantityOnHand = cfg.stock,
                        UpdatedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
