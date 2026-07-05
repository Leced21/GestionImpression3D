using Backend.Enums;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FactureController : ControllerBase
    {
        private readonly IFactureService _factureService;
        public FactureController(IFactureService factureService)
        {
            _factureService = factureService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var factures = await _factureService.GetAllAsync();
            return Ok(factures);
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var factures = await _factureService.GetByClientAsync(clientId);
            return Ok(factures);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var facture = await _factureService.GetByIdAsync(id);
            if (facture == null) return NotFound();
            return Ok(facture);
        }

        [HttpPatch("{id}/statut")]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] FactureStatus statut)
        {
            var facture = await _factureService.UpdateStatutAsync(id, statut);
            if (facture == null) return NotFound();
            return Ok(facture);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            var pdfBytes = await _factureService.GeneratePdfAsync(id);
            if (pdfBytes.Length == 0) return NotFound();
            return File(pdfBytes, "application/pdf", $"Facture_{id}.pdf");
        }
    }
}
