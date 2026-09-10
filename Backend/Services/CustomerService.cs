using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public CustomerService(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<PagedCustomerResultDto> GetCustomersAsync(int page, int pageSize, string? search = null)
        {
            var tenantId = _tenantContext.TenantId;
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 500);

            var query = _context.Customers
                .AsNoTracking()
                .Where(c => c.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Phone.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone,
                    CreatedAtUtc = c.CreatedAtUtc
                })
                .ToListAsync();

            return new PagedCustomerResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId <= 0)
            {
                throw new InvalidOperationException("Tenant context not available.");
            }

            var name = dto.Name.Trim();
            var phone = NormalizePhone(dto.Phone);

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Customer name is required.");
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new InvalidOperationException("Phone number is required.");
            }

            var phoneExists = await _context.Customers.AnyAsync(c =>
                c.TenantId == tenantId && c.Phone == phone);

            if (phoneExists)
            {
                throw new InvalidOperationException("A customer with this phone number already exists.");
            }

            var customer = new Customer
            {
                TenantId = tenantId,
                Name = name,
                Phone = phone,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return MapToDto(customer);
        }

        private static string NormalizePhone(string? phone)
        {
            return (phone ?? string.Empty).Trim();
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Phone = customer.Phone,
                CreatedAtUtc = customer.CreatedAtUtc
            };
        }
    }
}
