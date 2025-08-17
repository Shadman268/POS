namespace Backend.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        // For upload
        public IFormFile? Image { get; set; }

        // For response
        public string? ImagePath { get; set; }
    }
}
