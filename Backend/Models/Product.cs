namespace Backend.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
        // Store only the path (not the file itself)
        public string? ImagePath { get; set; }
    }
}
