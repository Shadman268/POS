using Backend.Services.Interfaces;
using System.Security.Claims;

namespace Backend.Services
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public int TenantId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
                return int.TryParse(claim, out var tenantId) ? tenantId : 0;
            }
        }

        public int? BranchId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("branchId")?.Value;
                return int.TryParse(claim, out var branchId) ? branchId : null;
            }
        }
    }
}
