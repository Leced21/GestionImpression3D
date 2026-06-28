using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaterialConsumptionController : ControllerBase
    {
        private readonly IMaterialConsumptionService _consumptionService;
        public MaterialConsumptionController(IMaterialConsumptionService consumptionService)
        {
            _consumptionService = consumptionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _consumptionService.GetAllAsync());
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            return Ok(await _consumptionService.GetStatisticsAsync(start, end));
        }

        [HttpGet("material/{materialId}")]
        public async Task<IActionResult> GetByMaterial(int materialId)
        {
            return Ok(await _consumptionService.GetByMaterialAsync(materialId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var consumption = await _consumptionService.GetByIdAsync(id);
            if (consumption == null) return NotFound();
            return Ok(consumption);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> Create([FromBody] CreateMaterialConsumptionRequest request)
        {
            try
            {
                var consumption = await _consumptionService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = consumption.Id }, consumption);
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
            var result = await _consumptionService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
