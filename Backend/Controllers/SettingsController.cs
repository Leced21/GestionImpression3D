using Backend.Data;
using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController: ControllerBase
    {
        private readonly IUserSettingsService _settingsService;
        private readonly IUserService _userService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SettingsController(IUserSettingsService settingsService, IUserService userService, AppDbContext context, IWebHostEnvironment env)
        {
            _settingsService = settingsService;
            _userService = userService;
            _context = context;
            _env = env;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // ==========================================
        // PROFIL
        // ==========================================

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.Id,
                user.Email,
                user.Nom,
                user.Prenom,
                user.Role,
                user.IsActive,
                user.DateCreation,
                FullName = $"{user.Prenom} {user.Nom}"
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                var user = await _settingsService.UpdateProfileAsync(userId, request);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // SÉCURITÉ
        // ==========================================

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _settingsService.ChangePasswordAsync(userId, request);
                return Ok(new { success = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("toggle-2fa")]
        public async Task<IActionResult> Toggle2FA()
        {
            var userId = GetCurrentUserId();
            var result = await _settingsService.Toggle2FAAsync(userId);
            return Ok(new { enabled = result });
        }

        // ==========================================
        // PARAMÈTRES
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var userId = GetCurrentUserId();
            var settings = await _settingsService.GetSettingsAsync(userId);
            return Ok(settings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto settings)
        {
            var userId = GetCurrentUserId();
            var updated = await _settingsService.UpdateSettingsAsync(userId, settings);
            return Ok(updated);
        }

        // ==========================================
        // SYSTÈME (Admin)
        // ==========================================

        [HttpGet("system-info")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemInfo()
        {
            var dbStatus = await _context.Database.CanConnectAsync() ? "🟢 Connectée" : "🔴 Déconnectée";

            string diskSpace;
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");
                var usedGo = (drive.TotalSize - drive.AvailableFreeSpace) / 1024.0 / 1024.0 / 1024.0;
                var totalGo = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                diskSpace = $"{usedGo:F1} Go / {totalGo:F1} Go";
            }
            catch (Exception)
            {
                diskSpace = "N/A";
            }

            return Ok(new
            {
                AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "N/A",
                Environment = _env.EnvironmentName,
                DbStatus = dbStatus,
                DiskSpace = diskSpace
            });
        }
    }
}
