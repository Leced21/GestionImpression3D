using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjetController:ControllerBase
    {
        private readonly IProjetService _projetService;
        public ProjetController(IProjetService projetService)
        {
            _projetService = projetService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var projets = await _projetService.GetAllAsync();
            return Ok(projets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var projet = await _projetService.GetByIdAsync(id);
            if (projet == null) return NotFound();
            return Ok(projet);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Projet projet)
        {
            var created = await _projetService.CreateAsync(projet);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Projet projet)
        {
            var updated = await _projetService.UpdateAsync(id, projet);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _projetService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/pieces")]
        public async Task<IActionResult> AjouterPiece(int id, [FromBody] AjouterPieceRequest request)
        {
            try
            {
                var result = await _projetService.AjouterPieceAsync(id, request.PieceId, request.Quantite);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}/pieces/{pieceId}")]
        public async Task<IActionResult> RetirerPiece(int id, int pieceId)
        {
            var result = await _projetService.RetirerPieceAsync(id, pieceId);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var stats = await _projetService.GetStatsAsync(id);
            return Ok(stats);
        }
        public class AjouterPieceRequest
        {
            public int PieceId { get; set; }
            public int Quantite { get; set; } = 1;
        }
    }
}
