using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserService _userService;

        // Note : IAuthService a été retiré car il n'était pas utilisé.
        public AdminController(IAuditLogRepository auditLogRepository, IUserService userService)
        {
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int? entityId, [FromQuery] string? entityType)
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

            // Comportement par défaut si aucun paramètre n'est fourni
            var logs = await _auditLogRepository.GetRecentAsync(100);
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
    }
}
