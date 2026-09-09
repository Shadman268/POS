using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IReceiptService
    {
        Task<Receipt> CreateReceiptAsync(ReceiptDto receiptDto);
        Task<Receipt> CreateAdjustmentAsync(AdjustReceiptDto request);
        Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync();
        Task<ReceiptDto?> GetReceiptByIdAsync(int id);
        Task<ReceiptDto?> GetAdjustableReceiptAsync(int id);
    }
}
