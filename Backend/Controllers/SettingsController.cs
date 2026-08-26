using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ITenantSettingsService _settingsService;

        public SettingsController(ITenantSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<ActionResult<TenantSettingsDto>> GetSettings()
        {
            return Ok(await _settingsService.GetSettingsAsync());
        }

        [HttpPut]
        public async Task<ActionResult<TenantSettingsDto>> UpdateSettings([FromBody] UpdateTenantSettingsDto dto)
        {
            return Ok(await _settingsService.UpdateSettingsAsync(dto));
        }
    }
}
