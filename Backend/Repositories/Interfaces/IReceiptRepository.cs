using Backend.Models;

namespace Backend.Repositories.Interfaces
{
    public interface IReceiptRepository
    {
        Task<Receipt> CreateReceiptAsync(Receipt receipt);
        Task<IEnumerable<Receipt>> GetAllReceiptsAsync();
        Task<Receipt?> GetReceiptByIdAsync(int id);
    }
}
