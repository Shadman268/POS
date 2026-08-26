namespace Backend.DTOs
{
    public class ResolvePosItemRequest
    {
        public int PosItemId { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; } = 1;
        public int? MedicineBatchId { get; set; }
    }

    public class ResolvePosItemResponse
    {
        public bool Success { get; set; }
        public bool RequiresPrice { get; set; }
        public string? Message { get; set; }
        public ProductDto? Item { get; set; }
    }

    public class MedicineImportResultDto
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
