using System.Text.Json.Serialization;

namespace Backend.DTOs
{
    /// <summary>
    /// Backward-compatible POS product shape for existing frontend.
    /// Id = TenantMedicine.Id when configured, otherwise CatalogIdOffset + MedicineId.
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int? TenantMedicineId { get; set; }
        public required string ProductName { get; set; }
        public string? GenericName { get; set; }
        public decimal Price { get; set; }
        public bool RequiresPrice { get; set; }
        public bool HasTenantPrice { get; set; }

        [JsonIgnore]
        public IFormFile? Image { get; set; }

        public string? ImagePath { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public int StockQuantity { get; set; }
        public string Unit { get; set; } = "Tablet";
        public bool IsStockTracked { get; set; }
    }
}
