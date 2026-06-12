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
    public class OrdresFabricationController: ControllerBase
    {
        private readonly IOrdreFabricationService _ordreService;
        public OrdresFabricationController(IOrdreFabricationService ordreService)
        {
            _ordreService = ordreService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ordres = await _ordreService.GetAllAsync();
            return Ok(ordres);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _ordreService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ordre = await _ordreService.GetByIdAsync(id);
            if (ordre == null) return NotFound();
            return Ok(ordre);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Create([FromBody] CreateOrdreRequest request)
        {
            try
            {
                var ordre = await _ordreService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = ordre.Id }, ordre);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrdreRequest request)
        {
            var ordre = await _ordreService.UpdateAsync(id, request);
            if (ordre == null) return NotFound();
            return Ok(ordre);
        }

        [HttpPatch("{id}/statut")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] OrdreStatut statut)
        {
            var ordre = await _ordreService.UpdateStatutAsync(id, statut);
            if (ordre == null) return NotFound();
            return Ok(ordre);
        }

        [HttpPost("{id}/start")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> StartProduction(int id)
        {
            var ordre = await _ordreService.StartProductionAsync(id);
            if (ordre == null) return NotFound();
            return Ok(ordre);
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,ProductionManager,Operator")]
        public async Task<IActionResult> CompleteProduction(int id)
        {
            var ordre = await _ordreService.CompleteProductionAsync(id);
            if (ordre == null) return NotFound();
            return Ok(ordre);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _ordreService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
