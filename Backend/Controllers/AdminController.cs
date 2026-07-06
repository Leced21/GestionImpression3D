using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAuditLogger _auditLogger;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IPermissionService _permissionService;
        private readonly IInvitationService _invitationService;

        // Note : IAuthService a été retiré car il n'était pas utilisé.
        public AdminController(IAuthService authService,
        IUserService userService,
        IAuditLogger auditLogger,
        IAuditLogRepository auditLogRepository,
        IPermissionService permissionService,
        IInvitationService invitationService)
        {
            _authService = authService;
            _userService = userService;
            _auditLogger = auditLogger;
            _auditLogRepository = auditLogRepository;
            _permissionService = permissionService;
            _invitationService = invitationService;
        }

        private const int MaxAuditLogLimit = 500;

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int? entityId,
            [FromQuery] string? entityType,
            [FromQuery] int? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? limit)
        {
            // Si des filtres de recherche sont fournis dans l'URL, on les applique
            if (entityId.HasValue || !string.IsNullOrEmpty(entityType))
            {
                if (!entityId.HasValue || string.IsNullOrEmpty(entityType))
                {
                    return BadRequest("Les paramètres entityId et entityType doivent être fournis ensemble.");
                }

                if (!Enum.TryParse<EntityType>(entityType, true, out var type))
                {
                    return BadRequest("Type d'entité invalide.");
                }

                var filteredLogs = await _auditLogRepository.GetByEntityAsync(type, entityId.Value);
                return Ok(filteredLogs);
            }

            if (userId.HasValue)
            {
                var userLogs = await _auditLogRepository.GetByUserAsync(userId.Value);
                return Ok(userLogs);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var rangeLogs = await _auditLogRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
                return Ok(rangeLogs);
            }

            // Comportement par défaut si aucun paramètre n'est fourni
            var effectiveLimit = Math.Clamp(limit ?? 100, 1, MaxAuditLogLimit);
            var logs = await _auditLogRepository.GetRecentAsync(effectiveLimit);
            return Ok(logs);
        }

        [HttpGet("audit-logs/entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityAuditLogs(string entityType, int entityId)
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest("Type d'entité invalide.");
            }

            var logs = await _auditLogRepository.GetByEntityAsync(type, entityId);
            return Ok(logs);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            if (request == null)
            {
                return BadRequest("Le corps de la requête ne peut pas être vide.");
            }

            try
            {
                var user = await _userService.UpdateRoleAsync(id, request.Role);
                if (user == null)
                {
                    return NotFound($"Utilisateur avec l'ID {id} non trouvé.");
                }

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("users/{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                var user = await _userService.UpdateStatusAsync(id, true);
                if (user == null)
                {
                    return NotFound($"Utilisateur avec l'ID {id} non trouvé.");
                }

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
        [HttpPut("users/{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                var user = await _userService.UpdateStatusAsync(id, false);
                if (user == null)
                {
                    return NotFound($"Utilisateur avec l'ID {id} non trouvé.");
                }

                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);
                if (!result) return NotFound();

                await _auditLogger.LogDeletionAsync(EntityType.User, id, $"Utilisateur {id}");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("users/{id}/permissions")]
        public async Task<IActionResult> GetUserPermissions(int id)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(id);
            return Ok(permissions);
        }

        [HttpGet("roles/{role}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string role)
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(role);
            return Ok(permissions);
        }

        // ==========================================
        // INVITATIONS
        // ==========================================

        [HttpPost("invitations")]
        public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var invitation = await _invitationService.CreateInvitationAsync(request.Email, request.Role, userId);

                await _auditLogger.LogCreationAsync(EntityType.Invitation, invitation.Id, invitation.Email);

                return Ok(new
                {
                    invitation.Id,
                    invitation.Email,
                    invitation.Role,
                    invitation.Token,
                    invitation.ExpiresAt,
                    invitation.CreatedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("invitations")]
        public async Task<IActionResult> GetPendingInvitations()
        {
            var invitations = await _invitationService.GetPendingInvitationsAsync();
            return Ok(invitations);
        }

        [HttpDelete("invitations/{id}")]
        public async Task<IActionResult> CancelInvitation(int id)
        {
            var result = await _invitationService.CancelInvitationAsync(id);
            if (!result) return NotFound();

            await _auditLogger.LogDeletionAsync(EntityType.Invitation, id, $"Invitation {id}");
            return NoContent();
        }

        [HttpPost("invitations/validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateInvitation([FromBody] string token)
        {
            var isValid = await _invitationService.ValidateInvitationAsync(token);
            return Ok(new { isValid });
        }

        [HttpPost("invitations/accept")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            try
            {
                var user = await _invitationService.AcceptInvitationAsync(request.Token, request.Password, request.Nom, request.Prenom);

                await _auditLogger.LogCreationAsync(EntityType.User, user.Id, user.Email);

                return Ok(new { message = "Inscription réussie", email = user.Email });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
