using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }

    public class PagedCustomerResultDto
    {
        public List<CustomerDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Phone { get; set; } = string.Empty;
    }
}
