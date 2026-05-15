using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PieceController:ControllerBase
    {
        private readonly IPieceService _pieceService;
        public PieceController(IPieceService pieceService)
        {
            _pieceService = pieceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Piece>>> GetAll()
        {
            var pieces = await _pieceService.GetAllAsync();
            return Ok(pieces);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Piece>> GetById(int id)
        {
            var piece = await _pieceService.GetByIdAsync(id);
            if (piece == null)
            {
                return NotFound();
            }
            return Ok(piece);
        }
        [HttpPost]
        public async Task<ActionResult<Piece>> Create(Piece piece)
        {
            try
            {
                var createdPiece = await _pieceService.CreateAsync(piece);
                return CreatedAtAction(nameof(GetById), new { id = createdPiece.Id }, createdPiece);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPatch("{id}/statut")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] string statut)
        {
            try
            {
                var piece = await _pieceService.UpdateStatutAsync(id, statut);
                if (piece == null)
                {
                    return NotFound();
                }
                return Ok(piece);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest (ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Piece piece)
        {
            if (id != piece.Id)
            {
                return BadRequest("ID mismatch");
            }
            try
            {
                var updatedPiece = await _pieceService.UpdateAsync(id, piece);
                if (updatedPiece == null)
                {
                    return NotFound();
                }
                return Ok(updatedPiece);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/prix-recommande")]
        public async Task<ActionResult<decimal>> GetPrixRecommandé(int id)
        {
            var prix = await _pieceService.CalculerPrixRecommandéAsync(id);
            if (prix == 0)
            {
                return NotFound();
            }
            return Ok(prix);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _pieceService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }
        // Controllers/PiecesController.cs - Ajouter cette méthode
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<DashboardStat>> GetDashboardStats()
        {
            var pieces = await _pieceService.GetAllAsync();

            var stats = new DashboardStat
            {
                TotalPieces = pieces.Count(),
                EnConception = pieces.Count(p => p.Statut == "Conception"),
                EnPrototypage = pieces.Count(p => p.Statut == "Prototypage"),
                EnProduction = pieces.Count(p => p.Statut == "Production"),
                Commercialisables = pieces.Count(p => p.Statut == "Commercialisable"),
                ChiffreAffaires = pieces.Where(p => p.Statut == "Commercialisable").Sum(p => p.PrixVente)
            };

            return Ok(stats);
        }
    }
}
