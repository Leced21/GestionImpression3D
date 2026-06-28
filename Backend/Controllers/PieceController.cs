using Backend.Enums;
using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PieceController:ControllerBase
    {
        private const long MaxUploadSizeBytes = 100 * 1024 * 1024;

        private readonly IPieceService _pieceService;
        private readonly IPdfExportService _pdfExportService;
        private readonly IExcelExportService _excelExportService;
        private readonly ISTLAnalyzerService _stlAnalyzerService;
        public PieceController(IPieceService pieceService, IPdfExportService pdfExportService, IExcelExportService excelExportService, ISTLAnalyzerService stlAnalyzerService)
        {
            _pieceService = pieceService;
            _pdfExportService = pdfExportService;
            _excelExportService = excelExportService;
            _stlAnalyzerService = stlAnalyzerService;
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
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
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
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] PieceStatus statut)
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
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
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
            var piece = await _pieceService.GetByIdAsync(id);
            if (piece == null)
            {
                return NotFound();
            }

            var prix = await _pieceService.CalculerPrixRecommandéAsync(id);
            return Ok(prix);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
                EnConception = pieces.Count(p => p.Statut == PieceStatus.Conception),
                EnPrototypage = pieces.Count(p => p.Statut == PieceStatus.Prototypage),
                EnProduction = pieces.Count(p => p.Statut == PieceStatus.Production),
                Commercialisables = pieces.Count(p => p.Statut == PieceStatus.Commercialisable),
                ChiffreAffaires = pieces.Where(p => p.Statut == PieceStatus.Commercialisable).Sum(p => p.PrixVente)
            };

            return Ok(stats);
        }
        [HttpPost("{id}/upload-stl")]
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
        public async Task<IActionResult> UploadStl (int id,  IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Aucun fichier fourni" });
            }

            if (file.Length > MaxUploadSizeBytes)
            {
                return BadRequest(new { error = "Fichier trop volumineux. Taille maximale: 100 Mo" });
            }

            var piece = await _pieceService.GetByIdAsync(id);
            if (piece == null) 
            {
                return NotFound(new { error = "Pièce non trouvé" });
            }
            var allowedExtensions = new[] { ".stl", ".step", ".3mf" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "Format non supporté. Utilisez STL, STEP ou 3MF" });
            }
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var safeReference = new string(piece.Reference.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(safeReference))
            {
                safeReference = $"piece{id}";
            }

            var fileName = $"{safeReference}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Mettre à jour la pièce
            piece.StlFileName = fileName;
            await _pieceService.UpdateAsync(id, piece);

            return Ok(new
            {
                fileName = fileName,
                filePath = $"/uploads/{fileName}",
                size = file.Length
            });
        }
        [HttpGet("{id}/stl")]
        public async Task<IActionResult> GetStlFile(int id)
        {
            var piece = await _pieceService.GetByIdAsync(id);
            if (piece == null || string.IsNullOrEmpty(piece.StlFileName))
                return NotFound(new { error = "Fichier STL non trouvé" });

            var safeFileName = Path.GetFileName(piece.StlFileName);
            if (!string.Equals(safeFileName, piece.StlFileName, StringComparison.Ordinal))
                return BadRequest(new { error = "Nom de fichier invalide" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", safeFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { error = "Fichier introuvable sur le serveur" });

            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 64 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            return File(stream, GetContentType(Path.GetExtension(safeFileName)), safeFileName, enableRangeProcessing: true);
        }
        // Controllers/PiecesController.cs - Ajouter
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var piece = await _pieceService.GetByIdAsync(id);
            if (piece == null) return NotFound();

            var pdfBytes = await _pdfExportService.ExportPieceToPdfAsync(piece);
            return File(pdfBytes, "application/pdf", $"Piece_{piece.Reference}.pdf");
        }
        [HttpGet("export/excel")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> ExportToExcel()
        {
            var pieces = await _pieceService.GetAllAsync();
            var excelBytes = await _excelExportService.ExportPiecesToExcelAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Pieces_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        [HttpPost("{id}/analyze-stl")]
        [Authorize(Roles = "Admin,Designer")]
        public async Task<IActionResult> AnalyzeSTL(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Aucun fichier fourni" });

            if (file.Length > MaxUploadSizeBytes)
                return BadRequest(new { error = "Fichier trop volumineux. Taille maximale: 100 Mo" });

            if (!string.Equals(Path.GetExtension(file.FileName), ".stl", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Seuls les fichiers STL peuvent être analysés" });

            var metadata = await _pieceService.AnalyzeSTLAsync(id, file);
            if (metadata == null) return NotFound();

            return Ok(metadata);
        }

        [HttpGet("{id}/stl-metadata")]
        public async Task<IActionResult> GetSTLMetadata(int id)
        {
            var metadata = await _stlAnalyzerService.GetMetadataByPieceAsync(id);
            if (metadata == null) return NotFound();
            return Ok(metadata);
        }

        private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
        {
            ".stl" => "model/stl",
            ".step" or ".stp" => "model/step",
            ".3mf" => "model/3mf",
            _ => "application/octet-stream"
        };
    }
}