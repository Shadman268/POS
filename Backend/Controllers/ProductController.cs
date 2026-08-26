using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Backward-compatible POS catalog endpoint. Search matches medicine name and generic name.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] string? brand)
        {
            var products = await _productService.GetAllProductsAsync(search, category, brand);
            return Ok(products);
        }

        /// <summary>
        /// Resolve adding a medicine to the sale cart. Returns RequiresPrice when tenant price is unset.
        /// </summary>
        [HttpPost("resolve")]
        public async Task<ActionResult<ResolvePosItemResponse>> ResolveProduct([FromBody] ResolvePosItemRequest request)
        {
            var result = await _productService.ResolvePosItemAsync(request);
            if (!result.Success && result.RequiresPrice)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, result);
            }

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
