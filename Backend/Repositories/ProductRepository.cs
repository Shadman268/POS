using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public ProductRepository(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => p.TenantId == _tenantContext.TenantId)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.TenantId = _tenantContext.TenantId;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
    }
}
