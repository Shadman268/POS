using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantMedicineController : ControllerBase
    {
        private readonly ITenantMedicineService _tenantMedicineService;

        public TenantMedicineController(ITenantMedicineService tenantMedicineService)
        {
            _tenantMedicineService = tenantMedicineService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedTenantMedicineResultDto>> GetTenantMedicines(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null)
        {
            var result = await _tenantMedicineService.GetTenantMedicinesAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TenantMedicineDto>> CreateTenantMedicine([FromBody] CreateTenantMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _tenantMedicineService.CreateTenantMedicineAsync(dto);
                return CreatedAtAction(nameof(GetTenantMedicines), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{medicineId}/settings")]
        public async Task<ActionResult<TenantMedicineDto>> UpsertSettings(
            int medicineId,
            [FromBody] UpdateTenantMedicineSettingsDto dto)
        {
            try
            {
                var result = await _tenantMedicineService.UpsertTenantMedicineSettingsAsync(medicineId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("by-medicine/{medicineId}")]
        public async Task<IActionResult> ResetTenantMedicineByMedicineId(int medicineId)
        {
            await _tenantMedicineService.ResetTenantMedicineByMedicineIdAsync(medicineId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTenantMedicine(int id)
        {
            try
            {
                await _tenantMedicineService.DeleteTenantMedicineAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
