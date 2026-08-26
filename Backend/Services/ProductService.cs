using Backend.DTOs;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class ProductService : IProductService
    {
        private readonly IPosCatalogService _posCatalogService;

        public ProductService(IPosCatalogService posCatalogService)
        {
            _posCatalogService = posCatalogService;
        }

        public Task<IEnumerable<ProductDto>> GetAllProductsAsync(string? search = null, string? category = null, string? brand = null)
        {
            return _posCatalogService.GetPosCatalogAsync(search, category, brand);
        }

        public Task<ResolvePosItemResponse> ResolvePosItemAsync(ResolvePosItemRequest request)
        {
            return _posCatalogService.ResolvePosItemAsync(request);
        }
    }
}
