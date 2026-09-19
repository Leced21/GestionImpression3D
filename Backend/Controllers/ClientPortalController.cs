using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    // Endpoints de données du portail client, en lecture seule. Authentifiés via le schéma
    // "ClientPortal" (audience JWT distincte des Users internes) : un token émis pour un User
    // interne ne peut jamais satisfaire cette exigence, et inversement.
    [ApiController]
    [Route("api/client-portal")]
    [Authorize(AuthenticationSchemes = "ClientPortal")]
    public class ClientPortalController : ControllerBase
    {
        private readonly IDevisService _devisService;
        private readonly IFactureService _factureService;
        private readonly ICommercialService _commercialService;

        public ClientPortalController(
            IDevisService devisService,
            IFactureService factureService,
            ICommercialService commercialService)
        {
            _devisService = devisService;
            _factureService = factureService;
            _commercialService = commercialService;
        }

        private bool TryGetCurrentClientId(out int clientId)
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out clientId) && clientId > 0;
        }

        [HttpGet("devis")]
        public async Task<IActionResult> GetDevis()
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var devis = await _devisService.GetByClientAsync(clientId);
            return Ok(devis);
        }

        [HttpGet("devis/{id}")]
        public async Task<IActionResult> GetDevisById(int id)
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var devis = await _devisService.GetByIdAsync(id);
            if (devis == null || devis.ClientId != clientId)
                return NotFound();

            return Ok(devis);
        }

        [HttpGet("factures")]
        public async Task<IActionResult> GetFactures()
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var factures = await _factureService.GetByClientAsync(clientId);
            return Ok(factures);
        }

        [HttpGet("factures/{id}")]
        public async Task<IActionResult> GetFactureById(int id)
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var facture = await _factureService.GetByIdAsync(id);
            if (facture == null || facture.ClientId != clientId)
                return NotFound();

            return Ok(facture);
        }

        [HttpGet("factures/{id}/pdf")]
        public async Task<IActionResult> GetFacturePdf(int id)
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var facture = await _factureService.GetByIdAsync(id);
            if (facture == null || facture.ClientId != clientId)
                return NotFound();

            var pdfBytes = await _factureService.GeneratePdfAsync(id);
            if (pdfBytes.Length == 0) return NotFound();

            return File(pdfBytes, "application/pdf", $"Facture_{facture.NumeroFacture}.pdf");
        }

        [HttpGet("commandes")]
        public async Task<IActionResult> GetCommandes()
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var commandes = await _commercialService.GetByClientAsync(clientId);
            return Ok(commandes);
        }

        [HttpGet("commandes/{id}")]
        public async Task<IActionResult> GetCommandeById(int id)
        {
            if (!TryGetCurrentClientId(out var clientId))
                return Unauthorized();

            var commande = await _commercialService.GetCommandeAsync(id);
            if (commande == null || commande.ClientId != clientId)
                return NotFound();

            return Ok(commande);
        }
    }
}
