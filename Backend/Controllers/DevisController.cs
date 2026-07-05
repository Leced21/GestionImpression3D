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
    public class DevisController:ControllerBase
    {
        private readonly IDevisService _devisService;
        public DevisController(IDevisService devisService)
        {
            _devisService = devisService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devis = await _devisService.GetAllAsync();
            return Ok(devis);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _devisService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var devis = await _devisService.GetByClientAsync(clientId);
            return Ok(devis);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var devis = await _devisService.GetByIdAsync(id);
            if (devis == null) return NotFound();
            return Ok(devis);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> Create([FromBody] CreateDevisRequest request)
        {
            try
            {
                var devis = await _devisService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = devis.Id }, devis);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/statut")]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] DevisStatus statut)
        {
            try
            {
                var devis = await _devisService.UpdateStatutAsync(id, statut);
                if (devis == null) return NotFound();
                return Ok(devis);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var pdfBytes = await _devisService.GeneratePdfAsync(id);
            if (pdfBytes.Length == 0) return NotFound();
            return File(pdfBytes, "application/pdf", $"Devis_{id}.pdf");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _devisService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
