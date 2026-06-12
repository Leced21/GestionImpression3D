using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrinterController: ControllerBase
    {
        private readonly IPrinterService _printerService;
        public PrinterController(IPrinterService printerService)
        {
            _printerService = printerService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var printers = await _printerService.GetAllAsync();
            return Ok(printers);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _printerService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var printer = await _printerService.GetByIdAsync(id);
            if (printer == null) return NotFound();
            return Ok(printer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePrinterRequest request)
        {
            try
            {
                var printer = await _printerService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = printer.Id }, printer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePrinterRequest request)
        {
            var printer = await _printerService.UpdateAsync(id, request);
            if (printer == null) return NotFound();
            return Ok(printer);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var printer = await _printerService.UpdateStatusAsync(id, status);
                if (printer == null) return NotFound();
                return Ok(printer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _printerService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
