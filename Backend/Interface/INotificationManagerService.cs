using Backend.Models;

namespace Backend.Interface
{
    public interface INotificationManagerService
    {
        Task<IEnumerable<AppNotification>> GetUserNotificationsAsync(int userId);
        Task<IEnumerable<AppNotification>> GetUnreadNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<AppNotification> SendAsync(int userId, string type, string title, string message, string? link = null, int? referenceId = null, string? referenceType = null);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<int> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int notificationId);
        Task<bool> DeleteAllAsync(int userId);

        // Notifications automatiques
        Task NotifyPrintJobStartedAsync(int userId, int jobId, string jobNumber);
        Task NotifyPrintJobCompletedAsync(int userId, int jobId, string jobNumber);
        Task NotifyPrintJobFailedAsync(int userId, int jobId, string jobNumber, string reason);
        Task NotifyLowStockAsync(int userId, int materialId, string materialName, decimal quantity);
        Task NotifyProjectStatusChangedAsync(int userId, int projectId, string projectName, string newStatus);
    }
}
