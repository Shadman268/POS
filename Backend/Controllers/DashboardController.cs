using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ITenantContext _tenantContext;

        public DashboardController(IDashboardService dashboardService, ITenantContext tenantContext)
        {
            _dashboardService = dashboardService;
            _tenantContext = tenantContext;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string period = "today")
        {
            if (_tenantContext.TenantId <= 0)
            {
                return Unauthorized(new { message = "Tenant context not available" });
            }

            var summary = await _dashboardService.GetSummaryAsync(_tenantContext.TenantId, period);
            return Ok(summary);
        }
    }
}
