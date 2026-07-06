using Backend.DTOs;
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
        private readonly ILogger<ProjetController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IPdfExportService _pdfExportService;
        private readonly IExcelExportService _excelExportService;
        public ProjetController(IProjetService projetService, ILogger<ProjetController> logger, IWebHostEnvironment env, IPdfExportService pdfExportService, IExcelExportService excelExportService)
        {
            _projetService = projetService;
            _logger = logger;
            _env = env;
            _pdfExportService = pdfExportService;
            _excelExportService = excelExportService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var projets = await _projetService.GetAllAsync();
                return Ok(projets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll projets");
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var projet = await _projetService.GetByIdAsync(id);
                if (projet == null) return NotFound();
                return Ok(projet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById {Id}", id);
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Projet projet)
        {
            try
            {
                var created = await _projetService.CreateAsync(projet);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating projet");
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Projet projet)
        {
            try
            {
                var updated = await _projetService.UpdateAsync(id, projet);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating projet {Id}", id);
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _projetService.DeleteAsync(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting projet {Id}", id);
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
            }
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding piece to project {Id}", id);
                return StatusCode(500, new { error = ex.Message, details = _env.IsDevelopment() ? ex.ToString() : null });
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
        // Controllers/ProjetsController.cs - Ajouter
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var projet = await _projetService.GetByIdAsync(id);
            if (projet == null) return NotFound();

            var pdfBytes = await _pdfExportService.ExportProjetToPdfAsync(projet);
            return File(pdfBytes, "application/pdf", $"Projet_{projet.Reference}.pdf");
        }

        [HttpGet("{id}/devis")]
        public async Task<IActionResult> ExportDevis(int id)
        {
            var projet = await _projetService.GetByIdAsync(id);
            if (projet == null) return NotFound();

            var pdfBytes = await _pdfExportService.ExportDevisToPdfAsync(projet);
            return File(pdfBytes, "application/pdf", $"Devis_{projet.Reference}.pdf");
        }

        [HttpGet("export/excel")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> ExportToExcel()
        {
            var projets = await _projetService.GetAllAsync();
            var excelBytes = await _excelExportService.ExportProjetsToExcelAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Projets_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet("{id}/technical-plans/pdf")]
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
        public async Task<IActionResult> GenerateTechnicalPlansPdf(int id)
        {
            try
            {
                var projet = await _projetService.GetByIdAsync(id);
                if (projet == null)
                    return NotFound(new { error = "Projet non trouvé" });

                var technicalPlanService = HttpContext.RequestServices.GetRequiredService<ITechnicalPlanService>();
                var pdfBytes = await technicalPlanService.GenerateProjectTechnicalPlansPdfAsync(id);

                return File(pdfBytes, "application/pdf", $"Plans-Techniques_{projet.Reference}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la génération des plans techniques pour le projet {id}");
                return StatusCode(500, new { error = "Erreur lors de la génération des plans", details = _env.IsDevelopment() ? ex.Message : null });
            }
        }

    }
}
