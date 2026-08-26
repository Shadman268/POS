namespace Backend.DTOs
{
    public class MedicineDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public string Unit { get; set; } = "Tablet";
        public string? Barcode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class PagedMedicineResultDto
    {
        public List<MedicineDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
