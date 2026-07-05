using Backend.Data;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetGlobalStats()
        {
            var stats = await _dashboardService.GetGlobalStatsAsync();
            return Ok(stats);
        }

        [HttpGet("production-trend")]
        public async Task<IActionResult> GetProductionTrend([FromQuery] int days = 30)
        {
            var trends = await _dashboardService.GetProductionTrendAsync(days);
            return Ok(trends);
        }

        [HttpGet("material-consumption")]
        public async Task<IActionResult> GetMaterialConsumption([FromQuery] int days = 30)
        {
            var consumption = await _dashboardService.GetMaterialConsumptionAsync(days);
            return Ok(consumption);
        }

        [HttpGet("printers-activity")]
        public async Task<IActionResult> GetPrintersActivity()
        {
            var activity = await _dashboardService.GetPrintersActivityAsync();
            return Ok(activity);
        }
    }
}
