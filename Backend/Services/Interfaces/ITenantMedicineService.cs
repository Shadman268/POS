using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface ITenantMedicineService
    {
        Task<PagedTenantMedicineResultDto> GetTenantMedicinesAsync(int page, int pageSize, string? search = null);
        Task<TenantMedicineDto> CreateTenantMedicineAsync(CreateTenantMedicineDto dto);
        Task<TenantMedicineDto> UpsertTenantMedicineSettingsAsync(int medicineId, UpdateTenantMedicineSettingsDto dto);
        Task DeleteTenantMedicineAsync(int id);
        Task ResetTenantMedicineByMedicineIdAsync(int medicineId);
    }
}
