using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(EntityType entityType, int entityId);
        Task<IEnumerable<AuditLog>> GetByUserAsync(int userId);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime start, DateTime end);
        Task<List<AuditLog>> GetRecentAsync(int count);
    }
}
