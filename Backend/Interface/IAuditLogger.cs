using Backend.Enums;

namespace Backend.Interface
{
    public interface IAuditLogger
    {
        Task LogCreationAsync(EntityType entityType, int entityId, string entityName);
        Task LogUpdateAsync(EntityType entityType, int entityId, string fieldName, string oldValue, string newValue);
        Task LogDeletionAsync(EntityType entityType, int entityId, string entityName);
        Task LogStatusChangeAsync(EntityType entityType, int entityId, string oldStatus, string newStatus);
    }
}
