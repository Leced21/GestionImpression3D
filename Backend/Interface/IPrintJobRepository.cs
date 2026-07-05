using Backend.Enums;
using Backend.Models;

namespace Backend.Interface
{
    public interface IPrintJobRepository
    {
        Task<PrintJob?> GetByIdAsync(int id);
        Task<IEnumerable<PrintJob>> GetAllAsync();
        Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintJobStatus status);
        Task<IEnumerable<PrintJob>> GetByPieceAsync(int pieceId);
        Task<IEnumerable<PrintJob>> GetByPrinterAsync(int printerId);
        Task<PrintJob> CreateAsync(PrintJob job);
        Task<PrintJob> UpdateAsync(PrintJob job);
        Task<bool> DeleteAsync(int id);
        Task<int> GetQueueCountAsync();
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
    }
}
