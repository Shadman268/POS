using AutoMapper;
using Backend.DTOs;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Backend.Models;

namespace Backend.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IMapper _mapper;
        public ProductService(IProductRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repository.GetAllProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            var createdProduct = await _repository.CreateProductAsync(product);
            return _mapper.Map<ProductDto>(createdProduct);
        }

    }
}
