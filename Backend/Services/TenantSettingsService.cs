using Backend.Data;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class TenantSettingsService : ITenantSettingsService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public TenantSettingsService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<TenantSettingsDto> GetSettingsAsync()
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstAsync(t => t.Id == _tenantContext.TenantId);

            return Map(tenant);
        }

        public async Task<TenantSettingsDto> UpdateSettingsAsync(UpdateTenantSettingsDto dto)
        {
            var tenant = await _context.Tenants.FirstAsync(t => t.Id == _tenantContext.TenantId);
            tenant.InventoryMode = dto.InventoryMode;
            tenant.PromptPriceWhenUnset = dto.PromptPriceWhenUnset;
            await _context.SaveChangesAsync();
            return Map(tenant);
        }

        private static TenantSettingsDto Map(Models.Tenant tenant)
        {
            return new TenantSettingsDto
            {
                InventoryMode = tenant.InventoryMode,
                PromptPriceWhenUnset = tenant.PromptPriceWhenUnset,
                ShopCode = tenant.ShopCode,
                Name = tenant.Name
            };
        }
    }
}
