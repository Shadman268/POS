using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ITenantContext _tenantContext;

        public CustomerController(ICustomerService customerService, ITenantContext tenantContext)
        {
            _customerService = customerService;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        public async Task<ActionResult<PagedCustomerResultDto>> GetCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null)
        {
            if (_tenantContext.TenantId <= 0)
            {
                return Unauthorized(new { message = "Tenant context not available" });
            }

            var result = await _customerService.GetCustomersAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            if (_tenantContext.TenantId <= 0)
            {
                return Unauthorized(new { message = "Tenant context not available" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _customerService.CreateCustomerAsync(dto);
                return CreatedAtAction(nameof(GetCustomers), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
