using Backend.DTOs;

namespace Backend.Interface
{
    public interface IPrintJobService
    {
        Task<IEnumerable<PrintJobDto>> GetAllAsync();
        Task<PrintJobDto?> GetByIdAsync(int id);
        Task<PrintJobDto> CreateAsync(CreatePrintJobRequest request);
        Task<PrintJobDto?> AssignPrinterAsync(int id, int printerId, int? operatorId = null);
        Task<PrintJobDto?> StartAsync(int id);
        Task<PrintJobDto?> PauseAsync(int id);
        Task<PrintJobDto?> ResumeAsync(int id);
        Task<PrintJobDto?> CompleteAsync(int id, UpdatePrintJobStatusRequest request);
        Task<PrintJobDto?> FailAsync(int id, string reason);
        Task<PrintJobDto?> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<PrintJobStatisticsDto> GetStatisticsAsync();
        Task<IEnumerable<PrintJobDto>> GetQueueAsync();
    }
}
