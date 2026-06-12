using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaterialStockController: ControllerBase
    {
        private readonly IMaterialStockService _materialStockService;
        public MaterialStockController(IMaterialStockService materialStockService)
        {
            _materialStockService = materialStockService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var materials = await _materialStockService.GetAllAsync();
            return Ok(materials);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _materialStockService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("alerts/low-stock")]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            var alerts = await _materialStockService.GetLowStockAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await _materialStockService.GetByIdAsync(id);
            if (material == null) return NotFound();
            return Ok(material);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialStockRequest request)
        {
            try
            {
                var material = await _materialStockService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/add")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> AddStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var material = await _materialStockService.AddStockAsync(id, request);
                if (material == null) return NotFound();
                return Ok(material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/remove")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> RemoveStock(int id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                var material = await _materialStockService.RemoveStockAsync(id, request);
                if (material == null) return NotFound();
                return Ok(material);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/thresholds")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> UpdateThresholds(int id, [FromBody] UpdateThresholdsRequest request)
        {
            var material = await _materialStockService.UpdateThresholdsAsync(id, request.MinThreshold, request.MaxThreshold);
            if (material == null) return NotFound();
            return Ok(material);
        }

        [HttpPatch("{id}/price")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdatePriceRequest request)
        {
            var material = await _materialStockService.UpdatePriceAsync(id, request.UnitPrice);
            if (material == null) return NotFound();
            return Ok(material);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _materialStockService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
