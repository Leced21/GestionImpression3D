using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/client-portal")]
    public class ClientPortalAuthController : ControllerBase
    {
        private readonly IClientPortalAuthService _clientPortalAuthService;
        public ClientPortalAuthController(IClientPortalAuthService clientPortalAuthService)
        {
            _clientPortalAuthService = clientPortalAuthService;
        }

        [HttpPost("request-access")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> RequestAccess([FromBody] RequestClientAccessRequest request)
        {
            await _clientPortalAuthService.RequestAccessAsync(request.Email);

            // Réponse volontairement identique que l'email corresponde ou non à un client,
            // pour ne pas permettre de deviner les adresses email enregistrées.
            return Ok(new { message = "Si cet email correspond à un compte, un lien d'accès vient d'être envoyé." });
        }

        [HttpPost("consume")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Consume([FromBody] ConsumeClientMagicLinkRequest request)
        {
            var response = await _clientPortalAuthService.ConsumeAsync(request.Token);
            if (response == null)
                return Unauthorized(new { error = "Lien invalide, déjà utilisé ou expiré." });

            return Ok(response);
        }
    }
}
