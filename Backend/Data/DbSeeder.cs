using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Tenants.AnyAsync())
            {
                var defaultTenant = new Tenant
                {
                    ShopCode = "demo",
                    Name = "Demo Shop",
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                context.Tenants.Add(defaultTenant);
                await context.SaveChangesAsync();
            }

            var tenant = await context.Tenants.FirstAsync(t => t.ShopCode == "demo");

            // Assign existing records without tenant to demo tenant
            var usersWithoutTenant = await context.Users.Where(u => u.TenantId == 0).ToListAsync();
            foreach (var user in usersWithoutTenant)
            {
                user.TenantId = tenant.Id;
            }

            var productsWithoutTenant = await context.Products.Where(p => p.TenantId == 0).ToListAsync();
            foreach (var product in productsWithoutTenant)
            {
                product.TenantId = tenant.Id;
            }

            var receiptsWithoutTenant = await context.Receipts.Where(r => r.TenantId == 0).ToListAsync();
            foreach (var receipt in receiptsWithoutTenant)
            {
                receipt.TenantId = tenant.Id;
            }

            await context.SaveChangesAsync();

            await SeedSampleProductsAsync(context, tenant.Id);
        }

        private static async Task SeedSampleProductsAsync(AppDbContext context, int tenantId)
        {
            if (await context.Products.AnyAsync(p => p.TenantId == tenantId && p.ExpiryDate != null))
            {
                return;
            }

            var today = DateTime.Today;
            var samples = new[]
            {
                new Product { TenantId = tenantId, ProductName = "Napa Extra 500mg", GenericName = "Paracetamol + Caffeine", Price = 12, BatchNumber = "NX-2291", ExpiryDate = today.AddDays(12), Category = "Medicine", Brand = "Beximco", StockQuantity = 86, Unit = "Tablet" },
                new Product { TenantId = tenantId, ProductName = "Seclo 20mg", GenericName = "Omeprazole", Price = 12, BatchNumber = "SC-8841", ExpiryDate = today.AddDays(18), Category = "Medicine", Brand = "Square", StockQuantity = 3, Unit = "Tablet" },
                new Product { TenantId = tenantId, ProductName = "Amodis 400mg", GenericName = "Albendazole", Price = 15, BatchNumber = "AM-1032", ExpiryDate = today.AddDays(21), Category = "Medicine", Brand = "General", StockQuantity = 60, Unit = "Tablet" },
                new Product { TenantId = tenantId, ProductName = "Fexo 120mg", GenericName = "Fexofenadine", Price = 10, BatchNumber = "FX-7710", ExpiryDate = today.AddDays(27), Category = "Medicine", Brand = "Incepta", StockQuantity = 0, Unit = "Tablet" },
                new Product { TenantId = tenantId, ProductName = "Insulin Mixtard", GenericName = "Insulin Human", Price = 450, BatchNumber = "IM-0092", ExpiryDate = today.AddDays(29), Category = "Medicine", Brand = "Novo Nordisk", StockQuantity = 24, Unit = "Vial" },
                new Product { TenantId = tenantId, ProductName = "ORSaline Sachet", GenericName = "Oral Rehydration Salts", Price = 10, Category = "Medicine", Brand = "General", StockQuantity = 200, Unit = "Sachet" },
                new Product { TenantId = tenantId, ProductName = "Vitamin C 500mg", GenericName = "Ascorbic Acid", Price = 20, Category = "Medicine", Brand = "General", StockQuantity = 150, Unit = "Tablet" },
                new Product { TenantId = tenantId, ProductName = "Digital BP Monitor", GenericName = "Blood Pressure Monitor", Price = 1450, Category = "Equipment", Brand = "Omron", StockQuantity = 12, Unit = "Unit" }
            };

            context.Products.AddRange(samples);
            await context.SaveChangesAsync();
        }
    }
}
