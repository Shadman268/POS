using Backend.Data;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public ReceiptRepository(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<Receipt> CreateReceiptAsync(Receipt receipt)
        {
            receipt.TenantId = _tenantContext.TenantId;
            if (_tenantContext.BranchId.HasValue)
            {
                receipt.BranchId = _tenantContext.BranchId;
            }

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();
            return receipt;
        }

        public async Task<IEnumerable<Receipt>> GetAllReceiptsAsync()
        {
            return await _context.Receipts
                .Where(r => r.TenantId == _tenantContext.TenantId)
                .Include(r => r.Items)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Receipt?> GetReceiptByIdAsync(int id)
        {
            return await _context.Receipts
                .Where(r => r.TenantId == _tenantContext.TenantId)
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
