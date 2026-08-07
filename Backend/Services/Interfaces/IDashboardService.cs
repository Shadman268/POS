using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(int tenantId, string period = "today");
    }
}
