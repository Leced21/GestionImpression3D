using Backend.Models;

namespace Backend.Interface
{
    public interface INotificationRepository
    {
        Task<AppNotification?> GetByIdAsync(int id);
        Task<IEnumerable<AppNotification>> GetByUserAsync(int userId);
        Task<IEnumerable<AppNotification>> GetUnreadByUserAsync(int userId);
        Task<AppNotification> CreateAsync(AppNotification notification);
        Task<AppNotification> UpdateAsync(AppNotification notification);
        Task<bool> MarkAsReadAsync(int id);
        Task<int> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int id);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> DeleteAllForUserAsync(int userId);
    }
}
