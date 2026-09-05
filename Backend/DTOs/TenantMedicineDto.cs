namespace Backend.DTOs
{
    public class TenantMedicineDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public string Unit { get; set; } = "Tablet";
        public string? Barcode { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public bool IsStockTracked { get; set; }
        public int StockQuantity { get; set; }
        public string? LocalSku { get; set; }
    }

    public class PagedTenantMedicineResultDto
    {
        public List<TenantMedicineDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateTenantMedicineDto
    {
        public required string Name { get; set; }
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string Category { get; set; } = "Medicine";
        public string Brand { get; set; } = "General";
        public string Unit { get; set; } = "Tablet";
        public string? Barcode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public bool IsStockTracked { get; set; }
        public string? LocalSku { get; set; }
        public int InitialStock { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateTenantMedicineSettingsDto
    {
        public decimal? SellingPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public bool IsStockTracked { get; set; }
        public int StockQuantity { get; set; }
    }
}
