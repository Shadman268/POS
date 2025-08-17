using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController: ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService service)
        {
            _productService = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromForm] ProductDto productDto)
        {
            System.Diagnostics.Debug.WriteLine(productDto);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var product = new Product
            {
                ProductName = productDto.ProductName,
                Price = productDto.Price
            };

            // Handle file upload
            if (productDto.Image != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, productDto.Image.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await productDto.Image.CopyToAsync(stream);

                // Save relative path in entity (for DB)
                product.ImagePath = Path.Combine("Uploads", productDto.Image.FileName).Replace("\\", "/");
            }

            // Save entity (with ImagePath) in DB
            var createdProduct = await _productService.CreateProductAsync(product);

            return CreatedAtAction(nameof(GetAllProducts), new { id = createdProduct.Id }, createdProduct);
        }


    }
}
