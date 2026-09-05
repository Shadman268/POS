using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Middleware
{
    public class DevelopmentAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public DevelopmentAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var disableAuth = _configuration.GetValue<bool>("Development:DisableAuth");

            if (disableAuth && context.User.Identity?.IsAuthenticated != true)
            {
                var shopCode = _configuration["Development:DemoShopCode"] ?? "demo";
                var username = _configuration["Development:DemoUsername"];

                var userQuery = dbContext.Users
                    .Include(u => u.Tenant)
                    .Where(u => u.Tenant.ShopCode == shopCode && u.Tenant.IsActive);

                var user = string.IsNullOrWhiteSpace(username)
                    ? await userQuery.FirstOrDefaultAsync()
                    : await userQuery.FirstOrDefaultAsync(u => u.Username == username);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, user.Username),
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new("sub", user.Id.ToString()),
                        new("tenantId", user.TenantId.ToString()),
                        new("shopCode", user.Tenant.ShopCode),
                        new(ClaimTypes.Role, user.Role.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, "Development");
                    context.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    var tenant = await dbContext.Tenants
                        .FirstOrDefaultAsync(t => t.ShopCode == shopCode && t.IsActive);

                    if (tenant != null)
                    {
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.Name, "dev"),
                            new(ClaimTypes.NameIdentifier, "0"),
                            new("sub", "0"),
                            new("tenantId", tenant.Id.ToString()),
                            new("shopCode", tenant.ShopCode),
                            new(ClaimTypes.Role, nameof(Models.UserRole.Admin))
                        };

                        var identity = new ClaimsIdentity(claims, "Development");
                        context.User = new ClaimsPrincipal(identity);
                    }
                }
            }

            await _next(context);
        }
    }
}
