using Backend.Interface;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
                return Unauthorized(new { error = "Email ou mot de passe incorrect" });

            return Ok(response);
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (await _authService.UserExistsAsync(request.Email))
                return BadRequest(new { error = "Cet email est déjà utilisé" });

            var response = await _authService.RegisterAsync(request);
            if (response == null)
                return BadRequest(new { error = "Erreur lors de l'inscription" });

            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new { user.Id, user.Email, user.Nom, user.Prenom, user.Role });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _authService.LogoutAsync(userId);
            return NoContent();
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { error = "Refresh token manquant" });

            var response = await _authService.RefreshAsync(request.RefreshToken);
            if (response == null)
                return Unauthorized(new { error = "Refresh token invalide ou expiré" });

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request.Email);
            // Réponse identique que le compte existe ou non : évite l'énumération d'emails.
            return Ok(new { message = "Si ce compte existe, un email de réinitialisation a été envoyé." });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
                return BadRequest(new { error = "Ce lien de réinitialisation est invalide ou a expiré." });

            return Ok(new { message = "Mot de passe réinitialisé avec succès." });
        }
    }
}
