using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintIncidentController:ControllerBase
    {
        private readonly IPrintIncidentService _incidentService;
        public PrintIncidentController(IPrintIncidentService incidentService)
        {
            _incidentService = incidentService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var incidents = await _incidentService.GetAllAsync();
            return Ok(incidents);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var stats = await _incidentService.GetStatisticsAsync(start, end);
            return Ok(stats);
        }

        [HttpGet("printer/{printerId}")]
        public async Task<IActionResult> GetByPrinter(int printerId)
        {
            var incidents = await _incidentService.GetByPrinterAsync(printerId);
            return Ok(incidents);
        }

        [HttpGet("printjob/{printJobId}")]
        public async Task<IActionResult> GetByPrintJob(int printJobId)
        {
            var incidents = await _incidentService.GetByPrintJobAsync(printJobId);
            return Ok(incidents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var incident = await _incidentService.GetByIdAsync(id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Create([FromBody] CreateIncidentRequest request)
        {
            var incident = await _incidentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] IncidentStatus status)
        {
            var incident = await _incidentService.UpdateStatusAsync(id, status);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Resolve(int id, [FromBody] ResolveIncidentRequest request)
        {
            var incident = await _incidentService.ResolveAsync(id, request);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _incidentService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
