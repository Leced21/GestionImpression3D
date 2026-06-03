using Azure.Core;
using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommercialController:ControllerBase
    {
        private readonly ICommercialService _commercialService;
        private readonly IExcelExportService _excelExportService;
        public CommercialController(ICommercialService commercialService, IExcelExportService excelExportService)
        {
            _commercialService = commercialService;
            _excelExportService = excelExportService;
        }
        [HttpGet("catalogue")]
        public async Task<IActionResult> GetCatalogue()
        {
            var catalogue = await _commercialService.GetCatalogueAsync();
            return Ok(catalogue);
        }

        [HttpPost("commande")]
        public async Task<IActionResult> CreerCommande([FromBody] CommandeRequest request)
        {
            try
            {
                var commande = await _commercialService.CreerCommandeAsync(request);
                return Ok(new
                {
                    message = "Commande créée avec succès",
                    commandeId = commande.Id,
                    numeroCommande = commande.NumeroCommande
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("commandes")]
        public async Task<IActionResult> GetCommandes()
        {
            var commandes = await _commercialService.GetAllCommandesAsync();
            return Ok(commandes);
        }

        [HttpGet("commandes/{id}")]
        public async Task<IActionResult> GetCommande(int id)
        {
            var commande = await _commercialService.GetCommandeAsync(id);
            if (commande == null)
                return NotFound();

            return Ok(commande);
        }

        [HttpPatch("commandes/{id}/statut")]
        public async Task<IActionResult> UpdateStatut(int id, [FromBody] string statut)
        {
            try
            {
                var commande = await _commercialService.UpdateStatutCommandeAsync(id, statut);
                if (commande == null)
                    return NotFound();

                return Ok(commande);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("commandes/{id}")]
        public async Task<IActionResult> AnnulerCommande(int id)
        {
            try
            {
                var result = await _commercialService.AnnulerCommandeAsync(id);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Commande annulée" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _commercialService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("chiffre-affaires")]
        public async Task<IActionResult> GetChiffreAffaires()
        {
            var ca = await _commercialService.GetChiffreAffairesAsync();
            return Ok(new { chiffreAffaires = ca });
        }
        [HttpGet("commandes/export/excel")]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> ExportCommandesToExcel()
        {
            var excelBytes = await _excelExportService.ExportCommandesToExcelAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Commandes_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
