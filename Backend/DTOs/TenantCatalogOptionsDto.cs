using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.DTOs
{
    public class TenantCatalogOptionsDto
    {
        public List<string> DosageForms { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Units { get; set; } = new();
    }

    public class AddTenantCatalogOptionDto
    {
        [Required]
        public CatalogOptionType Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
