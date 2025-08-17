using Backend.Models;
using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<Product> CreateProductAsync(Product product);
    }
}
