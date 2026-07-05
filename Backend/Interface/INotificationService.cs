namespace Backend.Interface
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string message, string? title = null);
        Task SendToGroupAsync(string group, string message, string? title = null);
        Task SendPrintJobStarted(int printJobId, string jobNumber);
        Task SendPrintJobCompleted(int printJobId, string jobNumber);
        Task SendPrintJobFailed(int printJobId, string jobNumber, string reason);
        Task SendLowStockAlert(int materialId, string materialName, decimal quantity);
    }
}
