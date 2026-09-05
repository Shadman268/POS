using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineCatalogService _catalogService;

        public MedicineController(IMedicineCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        /// <summary>
        /// One-time bulk upload of global medicine catalog from CSV.
        /// </summary>
        [HttpPost("import")]
        [RequestSizeLimit(100_000_000)]
        public async Task<ActionResult<MedicineImportResultDto>> ImportCatalog(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("CSV file is required.");
            }

            await using var stream = file.OpenReadStream();
            var result = await _catalogService.ImportFromCsvAsync(stream);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedMedicineResultDto>> GetMedicines(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null)
        {
            var result = await _catalogService.GetMedicinesAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            try
            {
                await _catalogService.DeleteMedicineAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
