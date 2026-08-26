using Backend.DTOs;
using Backend.Models.Enums;

namespace Backend.Services.Interfaces
{
    public interface IPosCatalogService
    {
        Task<IEnumerable<ProductDto>> GetPosCatalogAsync(string? search = null, string? category = null, string? brand = null);
        Task<ResolvePosItemResponse> ResolvePosItemAsync(ResolvePosItemRequest request);
    }

    public interface IMedicineCatalogService
    {
        Task<MedicineImportResultDto> ImportFromCsvAsync(Stream csvStream);
        Task<PagedMedicineResultDto> GetMedicinesAsync(int page, int pageSize, string? search = null);
    }

    public interface ITenantSettingsService
    {
        Task<TenantSettingsDto> GetSettingsAsync();
        Task<TenantSettingsDto> UpdateSettingsAsync(UpdateTenantSettingsDto dto);
    }
}
