using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantCatalogController : ControllerBase
    {
        private readonly ITenantCatalogService _tenantCatalogService;

        public TenantCatalogController(ITenantCatalogService tenantCatalogService)
        {
            _tenantCatalogService = tenantCatalogService;
        }

        [HttpGet("options")]
        public async Task<ActionResult<TenantCatalogOptionsDto>> GetOptions()
        {
            return Ok(await _tenantCatalogService.GetOptionsAsync());
        }

        [HttpPost("options")]
        public async Task<ActionResult<TenantCatalogOptionsDto>> AddOption([FromBody] AddTenantCatalogOptionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                return Ok(await _tenantCatalogService.AddOptionAsync(dto.Type, dto.Name));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
