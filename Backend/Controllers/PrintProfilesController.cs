using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrintProfilesController: ControllerBase
    {
        private readonly IPrintProfileService _profileService;
        public PrintProfilesController(IPrintProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _profileService.GetAllAsync();
            return Ok(profiles);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _profileService.GetStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("printer/{printerId}")]
        public async Task<IActionResult> GetByPrinter(int printerId)
        {
            var profiles = await _profileService.GetByPrinterAsync(printerId);
            return Ok(profiles);
        }

        [HttpGet("materiau/{materiau}")]
        public async Task<IActionResult> GetByMateriau(string materiau)
        {
            var profiles = await _profileService.GetByMateriauAsync(materiau);
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _profileService.GetByIdAsync(id);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Create([FromBody] CreatePrintProfileRequest request)
        {
            try
            {
                var profile = await _profileService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePrintProfileRequest request)
        {
            var profile = await _profileService.UpdateAsync(id, request);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPost("{id}/set-default")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var profile = await _profileService.SetDefaultAsync(id);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpPost("{id}/duplicate")]
        [Authorize(Roles = "Admin,ProductionManager")]
        public async Task<IActionResult> Duplicate(int id, [FromBody] string newName)
        {
            var profile = await _profileService.DuplicateAsync(id, newName);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _profileService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
