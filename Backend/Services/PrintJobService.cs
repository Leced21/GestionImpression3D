using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PrintJobService : IPrintJobService
    {
        private readonly IAuditLogger _auditLogger;
        private readonly IPieceRepository _pieceRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly IPrintJobRepository _printJobRepository;

        public PrintJobService(IAuditLogger auditLogger, IPieceRepository pieceRepository, IPrinterRepository printerRepository, IPrintJobRepository printJobRepository)
        {
            _auditLogger = auditLogger;
            _pieceRepository = pieceRepository;
            _printerRepository = printerRepository;
            _printJobRepository = printJobRepository;
        }
        public async Task<PrintJobDto?> AssignPrinterAsync(int id, int printerId, int? operatorId = null)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            var printer = await _printerRepository.GetByIdAsync(printerId);
            if (printer == null)
                throw new InvalidOperationException("Imprimante non trouvée");

            job.AssignToPrinter(printerId, operatorId);
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogUpdateAsync(EntityType.PrintJob, id, "PrinterId", "", printerId.ToString());

            return MapToDto(updated);
        }

        public async Task<PrintJobDto?> CancelAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            job.Cancel();
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, job.Status.ToString(), "Cancelled");

            return MapToDto(updated);
        }

        public async Task<PrintJobDto?> CompleteAsync(int id, UpdatePrintJobStatusRequest request)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            var actualDuration = request.ActualDurationMinutes ??
                (DateTime.UtcNow - job.StartedAt)?.Minutes ?? 0;

            var actualMaterial = request.ActualMaterialGrams ?? job.EstimatedMaterialGrams;

            job.Complete(actualDuration, actualMaterial);
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, "Printing", "Completed");

            // Mettre à jour le statut de l'imprimante
            if (job.PrinterId.HasValue)
            {
                var printer = await _printerRepository.GetByIdAsync(job.PrinterId.Value);
                printer?.CompletePrint(actualDuration);
                await _printerRepository.UpdateAsync(printer!);
            }

            return MapToDto(updated);
        }

        public async Task<PrintJobDto> CreateAsync(CreatePrintJobRequest request)
        {
            var piece = await _pieceRepository.GetByIdAsync(request.PieceId);
            if (piece == null)
                throw new InvalidOperationException("Pièce non trouvée");

            var priority = Enum.Parse<PrintJobPriority>(request.Priority);

            var job = PrintJob.Create(
                request.PieceId,
                request.Quantity,
                priority,
                request.EstimatedDurationMinutes,
                request.EstimatedMaterialGrams,
                request.Notes
            );

            var created = await _printJobRepository.CreateAsync(job);

            await _auditLogger.LogCreationAsync(EntityType.PrintJob, created.Id, created.JobNumber);

            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return false;

            var result = await _printJobRepository.DeleteAsync(id);

            if (result)
            {
                await _auditLogger.LogDeletionAsync(EntityType.PrintJob, id, job.JobNumber);
            }

            return result;
        }

        public async Task<PrintJobDto?> FailAsync(int id, string reason)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            job.Fail(reason);
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, "Printing", "Failed");

            return MapToDto(updated);
        }

        public async Task<IEnumerable<PrintJobDto>> GetAllAsync()
        {
            var jobs = await _printJobRepository.GetAllAsync();
            return jobs.Select(MapToDto);
        }

        public async Task<PrintJobDto?> GetByIdAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            return job != null ? MapToDto(job) : null;
        }

        public async Task<IEnumerable<PrintJobDto>> GetQueueAsync()
        {
            var jobs = await _printJobRepository.GetPendingJobsAsync();
            return jobs.Select(MapToDto);
        }

        public async Task<PrintJobStatisticsDto> GetStatisticsAsync()
        {
            var jobs = await _printJobRepository.GetAllAsync();

            return new PrintJobStatisticsDto
            {
                TotalJobs = jobs.Count(),
                PendingJobs = jobs.Count(j => j.Status == PrintJobStatus.Pending),
                QueuedJobs = jobs.Count(j => j.Status == PrintJobStatus.Queued),
                PrintingJobs = jobs.Count(j => j.Status == PrintJobStatus.Printing),
                CompletedJobs = jobs.Count(j => j.Status == PrintJobStatus.Completed),
                FailedJobs = jobs.Count(j => j.Status == PrintJobStatus.Failed),
                TotalDurationMinutes = jobs.Where(j => j.ActualDurationMinutes.HasValue).Sum(j => j.ActualDurationMinutes!.Value),
                TotalMaterialGrams = jobs.Where(j => j.ActualMaterialGrams > 0).Sum(j => j.ActualMaterialGrams),
                SuccessRate = jobs.Any() ? (decimal)jobs.Count(j => j.Status == PrintJobStatus.Completed) / jobs.Count() * 100 : 0
            };
        }

        public async Task<PrintJobDto?> PauseAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            job.Pause();
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, "Printing", "Paused");

            return MapToDto(updated);
        }

        public async Task<PrintJobDto?> ResumeAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            job.Resume();
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, "Paused", "Printing");

            return MapToDto(updated);
        }

        public async Task<PrintJobDto?> StartAsync(int id)
        {
            var job = await _printJobRepository.GetByIdAsync(id);
            if (job == null) return null;

            job.Start();
            var updated = await _printJobRepository.UpdateAsync(job);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintJob, id, "Queued", "Printing");

            // Mettre à jour le statut de l'imprimante
            if (job.PrinterId.HasValue)
            {
                var printer = await _printerRepository.GetByIdAsync(job.PrinterId.Value);
                printer?.StartPrint();
                await _printerRepository.UpdateAsync(printer!);
            }

            return MapToDto(updated);
        }

        private static PrintJobDto MapToDto(PrintJob job)
        {
            var progress = job.Quantity > 0 ? (int)((decimal)job.QuantityCompleted / job.Quantity * 100) : 0;

            return new PrintJobDto
            {
                Id = job.Id,
                JobNumber = job.JobNumber,
                PieceId = job.PieceId,
                PieceName = job.Piece?.Nom ?? string.Empty,
                PieceReference = job.Piece?.Reference ?? string.Empty,
                PrinterId = job.PrinterId,
                PrinterName = job.Printer?.Nom,
                OperatorId = job.OperatorId,
                OperatorName = job.Operator != null ? $"{job.Operator.Prenom} {job.Operator.Nom}" : null,
                Quantity = job.Quantity,
                QuantityCompleted = job.QuantityCompleted,
                Status = job.Status.ToString(),
                Priority = job.Priority.ToString(),
                CreatedAt = job.CreatedAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt,
                EstimatedDurationMinutes = job.EstimatedDurationMinutes,
                ActualDurationMinutes = job.ActualDurationMinutes,
                EstimatedMaterialGrams = job.EstimatedMaterialGrams,
                ActualMaterialGrams = job.ActualMaterialGrams,
                FailureReason = job.FailureReason,
                Notes = job.Notes,
                ProgressPercent = progress
            };
        }
    }
}
