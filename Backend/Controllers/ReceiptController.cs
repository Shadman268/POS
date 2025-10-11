using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceipt([FromBody] ReceiptDto receiptDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var receipt = await _receiptService.CreateReceiptAsync(receiptDto);
                return CreatedAtAction(nameof(GetReceiptById), new { id = receipt.Id }, receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiptDto>>> GetAllReceipts()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();
            return Ok(receipts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptDto>> GetReceiptById(int id)
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            return Ok(receipt);
        }
    }
}
