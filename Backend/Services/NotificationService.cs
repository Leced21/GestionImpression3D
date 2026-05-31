using Backend.Hubs;
using Backend.Interface;
using Microsoft.AspNetCore.SignalR;
using static Backend.Hubs.NotificationHub;

namespace Backend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendToUserAsync(int userId, string message, string? title = null)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationReceived", new
            {
                Type = "info",
                Title = title ?? "Information",
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendToGroupAsync(string group, string message, string? title = null)
        {
            await _hubContext.Clients.Group(group).SendAsync("NotificationReceived", new
            {
                Type = "info",
                Title = title ?? "Information",
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendPrintJobStarted(int printJobId, string jobNumber)
        {
            await NotificationHubExtensions.NotifyPrintJobStarted(_hubContext, printJobId, jobNumber);
        }

        public async Task SendPrintJobCompleted(int printJobId, string jobNumber)
        {
            await NotificationHubExtensions.NotifyPrintJobCompleted(_hubContext, printJobId, jobNumber);
        }

        public async Task SendPrintJobFailed(int printJobId, string jobNumber, string reason)
        {
            await NotificationHubExtensions.NotifyPrintJobFailed(_hubContext, printJobId, jobNumber, reason);
        }

        public async Task SendLowStockAlert(int materialId, string materialName, decimal quantity)
        {
            await NotificationHubExtensions.NotifyLowStock(_hubContext, materialId, materialName, quantity);
        }
    }
}
