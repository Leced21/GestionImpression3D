using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly IAuditLogRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public AuditLogger(IAuditLogRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task LogCreationAsync(EntityType entityType, int entityId, string entityName)
        {
            var log = AuditLog.CreateCreation(
                entityType: entityType,
                entityId: entityId,
                entityName: entityName ?? string.Empty,
                userId: _currentUser.UserId,
                userEmail: _currentUser.UserEmail ?? "Système",
                ipAddress: _currentUser.IpAddress ?? "N/A"
            );

            await _repository.AddAsync(log);
        }

        public async Task LogDeletionAsync(EntityType entityType, int entityId, string entityName)
        {
            var log = AuditLog.CreateDeletion(
                entityType: entityType,
                entityId: entityId,
                entityName: entityName ?? string.Empty,
                userId: _currentUser.UserId,
                userEmail: _currentUser.UserEmail ?? "Système",
                ipAddress: _currentUser.IpAddress ?? "N/A"
            );

            await _repository.AddAsync(log);
        }

        public async Task LogStatusChangeAsync(EntityType entityType, int entityId, string oldStatus, string newStatus)
        {
            var normalizedOld = oldStatus ?? string.Empty;
            var normalizedNew = newStatus ?? string.Empty;

            // Évite de journaliser si le statut n'a pas réellement changé
            if (string.Equals(normalizedOld, normalizedNew, StringComparison.Ordinal))
                return;

            var log = AuditLog.CreateStatusChange(
                entityType: entityType,
                entityId: entityId,
                oldStatus: normalizedOld,
                newStatus: normalizedNew,
                userId: _currentUser.UserId,
                userEmail: _currentUser.UserEmail ?? "Système",
                ipAddress: _currentUser.IpAddress ?? "N/A"
            );

            await _repository.AddAsync(log);
        }

        public async Task LogUpdateAsync(EntityType entityType, int entityId, string fieldName, string oldValue, string newValue)
        {
            var normalizedOld = oldValue ?? string.Empty;
            var normalizedNew = newValue ?? string.Empty;

            // Évite d'enregistrer une ligne d'audit si la valeur est identique
            if (string.Equals(normalizedOld, normalizedNew, StringComparison.Ordinal))
                return;

            var log = AuditLog.CreateUpdate(
                entityType: entityType,
                entityId: entityId,
                fieldName: fieldName ?? "Inconnu",
                oldValue: normalizedOld,
                newValue: normalizedNew,
                userId: _currentUser.UserId,
                userEmail: _currentUser.UserEmail ?? "Système",
                ipAddress: _currentUser.IpAddress ?? "N/A"
            );

            await _repository.AddAsync(log);
        }
    }
}