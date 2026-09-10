using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedCustomerResultDto> GetCustomersAsync(int page, int pageSize, string? search = null);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    }
}
