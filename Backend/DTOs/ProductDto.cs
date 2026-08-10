using System.Text.Json.Serialization;

namespace Backend.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public string? GenericName { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public IFormFile? Image { get; set; }

        public string? ImagePath { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public int StockQuantity { get; set; } = 100;
        public string Unit { get; set; } = "Tablet";
    }
}
