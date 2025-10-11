using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IReceiptService
    {
        Task<Receipt> CreateReceiptAsync(ReceiptDto receiptDto);
        Task<IEnumerable<ReceiptDto>> GetAllReceiptsAsync();
        Task<ReceiptDto?> GetReceiptByIdAsync(int id);
    }
}
