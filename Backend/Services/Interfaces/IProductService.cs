using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? search = null, string? category = null, string? brand = null);
        Task<ResolvePosItemResponse> ResolvePosItemAsync(ResolvePosItemRequest request);
    }
}
