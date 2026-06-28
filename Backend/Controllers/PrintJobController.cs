using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintJobController: ControllerBase
    {
        private readonly IPrintJobService _printJobService;
        private readonly IExcelExportService _excelExportService;
        public PrintJobController(IPrintJobService printJobService, IExcelExportService excelExportService)
        {
            _printJobService = printJobService;
            _excelExportService = excelExportService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var jobs = await _printJobService.GetAllAsync();
            return Ok(jobs);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _printJobService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetQueue()
        {
            var queue = await _printJobService.GetQueueAsync();
            return Ok(queue);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _printJobService.GetByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Create([FromBody] CreatePrintJobRequest request)
        {
            try
            {
                var job = await _printJobService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> AssignPrinter(int id, [FromBody] AssignPrinterRequest request)
        {
            try
            {
                var job = await _printJobService.AssignPrinterAsync(id, request.PrinterId, request.OperatorId);
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                var job = await _printJobService.StartAsync(id);
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/pause")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Pause(int id)
        {
            try
            {
                var job = await _printJobService.PauseAsync(id);
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/resume")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Resume(int id)
        {
            try
            {
                var job = await _printJobService.ResumeAsync(id);
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Complete(int id, [FromBody] UpdatePrintJobStatusRequest request)
        {
            try
            {
                var job = await _printJobService.CompleteAsync(id, request);
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/fail")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Fail(int id, [FromBody] string reason)
        {
            try
            {
                var job = await _printJobService.FailAsync(id, reason);
                if (job == null) return NotFound();
                return Ok(job);
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
                var job = await _printJobService.CancelAsync(id);
                if (job == null) return NotFound();
                return Ok(job);
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
            var result = await _printJobService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpGet("export/excel")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> ExportToExcel()
        {
            var jobs = await _printJobService.GetAllAsync();
            var excelBytes = await _excelExportService.ExportPrintJobsToExcelAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PrintJobs_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
