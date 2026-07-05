using Backend.Enums;

namespace Backend.Models
{
    public class AuditLog
    {
        public int Id { get; private set; }
        public int? UserId { get; private set; }
        public string? UserEmail { get; private set; }
        public ActionType Action { get; private set; }
        public EntityType EntityType { get; private set; }
        public int EntityId { get; private set; }
        public string? EntityName { get; private set; }
        public string? FieldName { get; private set; }
        public string? OldValue { get; private set; }
        public string? NewValue { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTime Timestamp { get; private set; }

        private AuditLog() { }

        // Factory methods - explicites et sans génériques
        public static AuditLog CreateCreation(
            EntityType entityType,
            int entityId,
            string entityName,
            int? userId = null,
            string? userEmail = null,
            string? ipAddress = null)
        {
            return new AuditLog
            {
                Action = ActionType.Create,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }

        public static AuditLog CreateUpdate(
            EntityType entityType,
            int entityId,
            string fieldName,
            string oldValue,
            string newValue,
            int? userId = null,
            string? userEmail = null,
            string? ipAddress = null)
        {
            return new AuditLog
            {
                Action = ActionType.Update,
                EntityType = entityType,
                EntityId = entityId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }

        public static AuditLog CreateDeletion(
            EntityType entityType,
            int entityId,
            string entityName,
            int? userId = null,
            string? userEmail = null,
            string? ipAddress = null)
        {
            return new AuditLog
            {
                Action = ActionType.Delete,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }

        public static AuditLog CreateStatusChange(
            EntityType entityType,
            int entityId,
            string oldStatus,
            string newStatus,
            int? userId = null,
            string? userEmail = null,
            string? ipAddress = null)
        {
            return new AuditLog
            {
                Action = ActionType.StatusChange,
                EntityType = entityType,
                EntityId = entityId,
                FieldName = "Statut",
                OldValue = oldStatus,
                NewValue = newStatus,
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
