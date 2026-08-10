namespace Backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public required string ProductName { get; set; }
        public string? GenericName { get; set; }
        public decimal Price { get; set; }
        // Store only the path (not the file itself)
        public string? ImagePath { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public int StockQuantity { get; set; } = 100;
        public string Unit { get; set; } = "Tablet";

        public Tenant Tenant { get; set; } = null!;
    }
}
