using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PieceVersionsController: ControllerBase
    {
        private readonly IPieceVersionService _versionService;
        private readonly IPieceService _pieceService;
        public PieceVersionsController(IPieceVersionService versionService, IPieceService pieceService)
        {
            _versionService = versionService;
            _pieceService = pieceService;
        }

        [HttpGet("piece/{pieceId}")]
        public async Task<IActionResult> GetVersionsByPiece(int pieceId)
        {
            var versions = await _versionService.GetVersionsByPieceAsync(pieceId);
            return Ok(versions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVersion(int id)
        {
            var version = await _versionService.GetVersionAsync(id);
            if (version == null) return NotFound();
            return Ok(version);
        }

        [HttpPost("piece/{pieceId}")]
        [Authorize(Roles = "Admin,Designer")]
        public async Task<IActionResult> CreateVersion(int pieceId, [FromBody] CreateVersionRequest request)
        {
            var piece = await _pieceService.GetByIdAsync(pieceId);
            if (piece == null) return NotFound();

            var createdBy = User.Identity?.Name ?? "System";
            var version = await _versionService.CreateVersionAsync(pieceId, piece, request.ChangeLog, createdBy, request.IsPrototype);

            return Ok(version);
        }

        [HttpPost("{id}/promote")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> PromoteToProduction(int id)
        {
            var version = await _versionService.PromoteToProductionAsync(id);
            if (version == null) return NotFound();
            return Ok(version);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareVersions([FromQuery] int v1, [FromQuery] int v2)
        {
            var hasChanges = await _versionService.CompareVersionsAsync(v1, v2);
            return Ok(new { hasChanges });
        }
    }
}
