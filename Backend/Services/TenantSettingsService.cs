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

            if (dto.Name != null)
            {
                tenant.Name = dto.Name.Trim();
            }

            if (dto.MaintainStock.HasValue)
            {
                tenant.MaintainStock = dto.MaintainStock.Value;
            }

            if (dto.PromptPriceWhenUnset.HasValue)
            {
                tenant.PromptPriceWhenUnset = dto.PromptPriceWhenUnset.Value;
            }

            if (dto.ReceiptHeader != null)
            {
                tenant.ReceiptHeader = string.IsNullOrWhiteSpace(dto.ReceiptHeader)
                    ? null
                    : dto.ReceiptHeader.Trim();
            }

            if (dto.ReceiptFooter != null)
            {
                tenant.ReceiptFooter = string.IsNullOrWhiteSpace(dto.ReceiptFooter)
                    ? null
                    : dto.ReceiptFooter.Trim();
            }

            if (dto.ShowLineDiscount.HasValue)
            {
                tenant.ShowLineDiscount = dto.ShowLineDiscount.Value;
            }

            if (dto.ShowVat.HasValue)
            {
                tenant.ShowVat = dto.ShowVat.Value;
            }

            if (dto.VatPercent.HasValue)
            {
                tenant.VatPercent = Math.Clamp(dto.VatPercent.Value, 0, 100);
            }

            await _context.SaveChangesAsync();
            return Map(tenant);
        }

        private static TenantSettingsDto Map(Models.Tenant tenant)
        {
            return new TenantSettingsDto
            {
                InventoryMode = tenant.InventoryMode,
                PromptPriceWhenUnset = tenant.PromptPriceWhenUnset,
                MaintainStock = tenant.MaintainStock,
                ReceiptHeader = tenant.ReceiptHeader,
                ReceiptFooter = tenant.ReceiptFooter,
                ShowLineDiscount = tenant.ShowLineDiscount,
                ShowVat = tenant.ShowVat,
                VatPercent = tenant.VatPercent,
                ShopCode = tenant.ShopCode,
                Name = tenant.Name
            };
        }
    }
}
