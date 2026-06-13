using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrinterMaintenanceController : ControllerBase
    {
        private readonly IPrinterMaintenanceService _maintenanceService;
        public PrinterMaintenanceController(IPrinterMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _maintenanceService.GetAllAsync());
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcoming([FromQuery] int days = 7)
        {
            return Ok(await _maintenanceService.GetUpcomingAsync(days));
        }

        [HttpGet("printer/{printerId}")]
        public async Task<IActionResult> GetByPrinter(int printerId)
        {
            return Ok(await _maintenanceService.GetByPrinterAsync(printerId));
        }

        [HttpGet("printer/{printerId}/statistics")]
        public async Task<IActionResult> GetStatistics(int printerId)
        {
            return Ok(await _maintenanceService.GetStatisticsAsync(printerId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var maintenance = await _maintenanceService.GetByIdAsync(id);
            if (maintenance == null) return NotFound();
            return Ok(maintenance);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Create([FromBody] CreatePrinterMaintenanceRequest request)
        {
            try
            {
                var maintenance = await _maintenanceService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = maintenance.Id }, maintenance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePrinterMaintenanceRequest request)
        {
            try
            {
                var maintenance = await _maintenanceService.UpdateAsync(id, request);
                if (maintenance == null) return NotFound();
                return Ok(maintenance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Complete(int id, [FromBody] CompleteMaintenanceRequest request)
        {
            try
            {
                var maintenance = await _maintenanceService.CompleteAsync(id, request);
                if (maintenance == null) return NotFound();
                return Ok(maintenance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var maintenance = await _maintenanceService.CancelAsync(id);
                if (maintenance == null) return NotFound();
                return Ok(maintenance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _maintenanceService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
