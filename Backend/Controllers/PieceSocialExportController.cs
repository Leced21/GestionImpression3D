using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/pieces/{pieceId:int}/social-export")]
    [Authorize]
    public class PieceSocialExportController : ControllerBase
    {
        private readonly IPieceSocialExportService _socialExportService;

        public PieceSocialExportController(IPieceSocialExportService socialExportService)
        {
            _socialExportService = socialExportService;
        }

        [HttpGet("png")]
        public async Task<IActionResult> ExportPng(int pieceId, [FromQuery] string format = "square")
        {
            var bytes = await _socialExportService.ExportProductPostPngAsync(pieceId, format);
            if (bytes == null)
            {
                return NotFound();
            }

            return File(bytes, "image/png", BuildFileName(pieceId, format, "png"));
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> ExportPdf(int pieceId, [FromQuery] string format = "square")
        {
            var bytes = await _socialExportService.ExportProductPostPdfAsync(pieceId, format);
            if (bytes == null)
            {
                return NotFound();
            }

            return File(bytes, "application/pdf", BuildFileName(pieceId, format, "pdf"));
        }

        private static string BuildFileName(int pieceId, string format, string extension)
        {
            var safeFormat = string.Equals(format, "story", StringComparison.OrdinalIgnoreCase) ? "story" : "square";
            return $"piece-{pieceId}-social-{safeFormat}.{extension}";
        }
    }
}
