using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Backend.Hubs
{
    [Authorize]
    public class NotificationHub:Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, List<string>> _userGroups = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.Remove(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!_userGroups.ContainsKey(userId))
                    _userGroups[userId] = new List<string>();

                if (!_userGroups[userId].Contains(groupName))
                    _userGroups[userId].Add(groupName);
            }
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && _userGroups.ContainsKey(userId))
            {
                _userGroups[userId].Remove(groupName);
            }
        }
        // Classe statique pour les méthodes d'envoi
        public static class NotificationHubExtensions
        {
            // Méthodes pour envoyer des notifications à des groupes spécifiques
            public static async Task NotifyPrintJobStarted(IHubContext<NotificationHub> hubContext, int printJobId, string jobNumber)
            {
                await hubContext.Clients.Group("production").SendAsync("PrintJobStarted", new
                {
                    Id = printJobId,
                    JobNumber = jobNumber,
                    Status = "Printing",
                    Timestamp = DateTime.UtcNow
                });

                await hubContext.Clients.All.SendAsync("NotificationReceived", new
                {
                    Type = "info",
                    Title = "Impression démarrée",
                    Message = $"Le job {jobNumber} a démarré",
                    Timestamp = DateTime.UtcNow
                });
            }

            public static async Task NotifyPrintJobCompleted(IHubContext<NotificationHub> hubContext, int printJobId, string jobNumber)
            {
                await hubContext.Clients.Group("production").SendAsync("PrintJobCompleted", new
                {
                    Id = printJobId,
                    JobNumber = jobNumber,
                    Status = "Completed",
                    Timestamp = DateTime.UtcNow
                });

                await hubContext.Clients.All.SendAsync("NotificationReceived", new
                {
                    Type = "success",
                    Title = "Impression terminée",
                    Message = $"Le job {jobNumber} est terminé",
                    Timestamp = DateTime.UtcNow
                });
            }

            public static async Task NotifyPrintJobFailed(IHubContext<NotificationHub> hubContext, int printJobId, string jobNumber, string reason)
            {
                await hubContext.Clients.Group("production").SendAsync("PrintJobFailed", new
                {
                    Id = printJobId,
                    JobNumber = jobNumber,
                    Status = "Failed",
                    Reason = reason,
                    Timestamp = DateTime.UtcNow
                });

                await hubContext.Clients.All.SendAsync("NotificationReceived", new
                {
                    Type = "error",
                    Title = "Échec d'impression",
                    Message = $"Le job {jobNumber} a échoué: {reason}",
                    Timestamp = DateTime.UtcNow
                });
            }

            public static async Task NotifyLowStock(IHubContext<NotificationHub> hubContext, int materialId, string materialName, decimal quantity)
            {
                await hubContext.Clients.Group("stock").SendAsync("LowStockAlert", new
                {
                    Id = materialId,
                    Name = materialName,
                    Quantity = quantity,
                    Timestamp = DateTime.UtcNow
                });

                await hubContext.Clients.Group("admin").SendAsync("NotificationReceived", new
                {
                    Type = "warning",
                    Title = "Stock bas",
                    Message = $"Le matériau {materialName} atteint un niveau bas ({quantity})",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
