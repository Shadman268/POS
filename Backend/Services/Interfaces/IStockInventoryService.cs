using Backend.Models;
using Backend.Models.Enums;

namespace Backend.Services.Interfaces
{
    public interface IStockInventoryService
    {
        Task DeductForSaleAsync(Tenant tenant, ReceiptItem item, int receiptId);

        Task<int?> TryGetAvailableStockAsync(Tenant tenant, TenantMedicine tenantMedicine);
    }

    public sealed class BatchDeductionResult
    {
        public int BatchId { get; init; }
        public string BatchNumber { get; init; } = string.Empty;
        public DateTime ExpiryDate { get; init; }
    }
}
