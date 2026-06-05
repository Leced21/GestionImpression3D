using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class NotificationManagerService : INotificationManagerService
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationManagerService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }
        public async Task<bool> DeleteAllAsync(int userId)
        {
            return await _notificationRepository.DeleteAllForUserAsync(userId);
        }

        public async Task<bool> DeleteAsync(int notificationId)
        {
            return await _notificationRepository.DeleteAsync(notificationId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<IEnumerable<AppNotification>> GetUnreadNotificationsAsync(int userId)
        {
            return await _notificationRepository.GetUnreadByUserAsync(userId);
        }

        public async Task<IEnumerable<AppNotification>> GetUserNotificationsAsync(int userId)
        {
            return await _notificationRepository.GetByUserAsync(userId);
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task NotifyLowStockAsync(int userId, int materialId, string materialName, decimal quantity)
        {
            await SendAsync(userId, "warning", "Stock bas",
                $"Le matériau {materialName} est bas ({quantity})", $"/stock/{materialId}", materialId, "MaterialStock");
        }

        public async Task NotifyPrintJobCompletedAsync(int userId, int jobId, string jobNumber)
        {
            await SendAsync(userId, "success", "Impression terminée",
                $"Le job {jobNumber} est terminé", $"/print-jobs/{jobId}", jobId, "PrintJob");
        }

        public async Task NotifyPrintJobFailedAsync(int userId, int jobId, string jobNumber, string reason)
        {
            await SendAsync(userId, "error", "Échec d'impression",
                $"Le job {jobNumber} a échoué: {reason}", $"/print-jobs/{jobId}", jobId, "PrintJob");
        }

        public async Task NotifyPrintJobStartedAsync(int userId, int jobId, string jobNumber)
        {
            await SendAsync(userId, "info", "Impression démarrée",
                $"Le job {jobNumber} a démarré", $"/print-jobs/{jobId}", jobId, "PrintJob");
        }

        public async Task NotifyProjectStatusChangedAsync(int userId, int projectId, string projectName, string newStatus)
        {
            await SendAsync(userId, "info", "Projet mis à jour",
            $"Le projet {projectName} est passé en {newStatus}", $"/projets/{projectId}", projectId, "Projet");
        }

        public async Task<AppNotification> SendAsync(int userId, string type, string title, string message, string? link = null, int? referenceId = null, string? referenceType = null)
        {
            var notification = new AppNotification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                Link = link,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            return await _notificationRepository.CreateAsync(notification);
        }
    }
}
