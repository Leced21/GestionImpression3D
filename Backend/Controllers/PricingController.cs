using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/pricing")]
    [Authorize]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpGet("settings")]
        public async Task<ActionResult<PricingSettingsDto>> GetSettings()
        {
            return Ok(await _pricingService.GetSettingsAsync());
        }

        [HttpPut("settings")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<ActionResult<PricingSettingsDto>> UpdateSettings(UpdatePricingSettingsRequest request)
        {
            try
            {
                return Ok(await _pricingService.UpdateSettingsAsync(request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("pieces/{pieceId:int}")]
        public async Task<ActionResult<PiecePricingDto>> GetPiecePricing(int pieceId)
        {
            var pricing = await _pricingService.GetPiecePricingAsync(pieceId);
            return pricing == null ? NotFound() : Ok(pricing);
        }

        [HttpPut("pieces/{pieceId:int}")]
        [Authorize(Roles = "Admin,Designer,ProductionManager")]
        public async Task<ActionResult<PiecePricingDto>> UpdatePieceCosting(int pieceId, UpdatePieceCostingRequest request)
        {
            try
            {
                var pricing = await _pricingService.UpdatePieceCostingAsync(pieceId, request);
                return pricing == null ? NotFound() : Ok(pricing);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ProfitabilityDashboardDto>> GetDashboard()
        {
            return Ok(await _pricingService.GetDashboardAsync());
        }
    }
}
