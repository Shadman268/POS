using Backend.DTOs;
using Backend.Models.Enums;

namespace Backend.Services.Interfaces
{
    public interface ITenantCatalogService
    {
        Task<TenantCatalogOptionsDto> GetOptionsAsync();
        Task<TenantCatalogOptionsDto> AddOptionAsync(CatalogOptionType type, string name);
        Task EnsureOptionsAsync(string? dosageForm, string? brand, string? unit);
    }
}
