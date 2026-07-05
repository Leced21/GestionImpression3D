using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PrinterMaintenanceService : IPrinterMaintenanceService
    {
        private readonly IPrinterMaintenanceRepository _maintenanceRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly IAuditLogger _auditLogger;
        private readonly ICurrentUserService _currentUser;
        public PrinterMaintenanceService(IPrinterMaintenanceRepository maintenanceRepository, IPrinterRepository printerRepository, IAuditLogger auditLogger, ICurrentUserService currentUser)
        {
            _maintenanceRepository = maintenanceRepository;
            _printerRepository = printerRepository;
            _auditLogger = auditLogger;
            _currentUser = currentUser;
        }
        public async Task<PrinterMaintenance?> CancelAsync(int id)
        {
            var maintenance = await _maintenanceRepository.GetByIdAsync(id);
            if (maintenance == null) return null;

            if (maintenance.Status != MaintenanceStatus.Scheduled && maintenance.Status != MaintenanceStatus.InProgress)
                throw new InvalidOperationException("Seule une maintenance planifiée ou en cours peut être annulée");

            var previousStatus = maintenance.Status;
            maintenance.Status = MaintenanceStatus.Cancelled;
            var updated = await _maintenanceRepository.UpdateAsync(maintenance);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrinterMaintenance, id, previousStatus.ToString(), MaintenanceStatus.Cancelled.ToString());

            return updated;
        }

        public async Task<PrinterMaintenance?> CompleteAsync(int id, CompleteMaintenanceRequest request)
        {
            var maintenance = await _maintenanceRepository.GetByIdAsync(id);
            if (maintenance == null) return null;

            if (maintenance.Status == MaintenanceStatus.Completed)
                throw new InvalidOperationException("Cette maintenance est déjà terminée");
            if (maintenance.Status == MaintenanceStatus.Cancelled)
                throw new InvalidOperationException("Une maintenance annulée ne peut pas être terminée");

            var previousStatus = maintenance.Status;
            var completed = await _maintenanceRepository.CompleteAsync(id, request.Notes, request.PerformedBy ?? _currentUser.UserEmail);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrinterMaintenance, id, previousStatus.ToString(), MaintenanceStatus.Completed.ToString());

            return completed;
        }

        public async Task<PrinterMaintenance> CreateAsync(CreatePrinterMaintenanceRequest request)
        {
            ValidateMaintenanceRequest(request.Title, request.DurationMinutes, request.Cost);

            var printer = await _printerRepository.GetByIdAsync(request.PrinterId);
            if (printer == null)
                throw new InvalidOperationException("Imprimante non trouvée");

            var maintenance = new PrinterMaintenance
            {
                PrinterId = request.PrinterId,
                Type = request.Type,
                Title = request.Title,
                Description = request.Description,
                ScheduledDate = request.ScheduledDate.ToUniversalTime(),
                DurationMinutes = request.DurationMinutes,
                Cost = request.Cost,
                Notes = request.Notes,
                Status = MaintenanceStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _maintenanceRepository.CreateAsync(maintenance);

            await _auditLogger.LogCreationAsync(EntityType.PrinterMaintenance, created.Id,
                $"Maintenance {request.Type} sur {printer.Nom}: {request.Title}");

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var maintenance = await _maintenanceRepository.GetByIdAsync(id);
            if (maintenance == null) return false;

            var result = await _maintenanceRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.PrinterMaintenance, id, maintenance.Title);

            return result;
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetAllAsync()
        {
            return await _maintenanceRepository.GetAllAsync();
        }

        public async Task<PrinterMaintenance?> GetByIdAsync(int id)
        {
            return await _maintenanceRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetByPrinterAsync(int printerId)
        {
            return await _maintenanceRepository.GetByPrinterAsync(printerId);
        }

        public async Task<PrinterMaintenanceStatisticsDto> GetStatisticsAsync(int printerId)
        {
            return await _maintenanceRepository.GetStatisticsAsync(printerId);
        }

        public async Task<IEnumerable<PrinterMaintenance>> GetUpcomingAsync(int days = 7)
        {
            return await _maintenanceRepository.GetUpcomingAsync(days);
        }

        public async Task<PrinterMaintenance?> UpdateAsync(int id, UpdatePrinterMaintenanceRequest request)
        {
            var maintenance = await _maintenanceRepository.GetByIdAsync(id);
            if (maintenance == null) return null;

            if (maintenance.Status == MaintenanceStatus.Completed || maintenance.Status == MaintenanceStatus.Cancelled)
                throw new InvalidOperationException("Une maintenance terminée ou annulée ne peut plus être modifiée");

            ValidateMaintenanceRequest(request.Title, request.DurationMinutes, request.Cost);

            maintenance.Title = request.Title;
            maintenance.Description = request.Description;
            maintenance.ScheduledDate = request.ScheduledDate.ToUniversalTime();
            maintenance.DurationMinutes = request.DurationMinutes;
            maintenance.Cost = request.Cost;
            maintenance.Notes = request.Notes;

            var updated = await _maintenanceRepository.UpdateAsync(maintenance);

            await _auditLogger.LogUpdateAsync(EntityType.PrinterMaintenance, id, "Maintenance", "Modifié", updated.Title);

            return updated;
        }

        private static void ValidateMaintenanceRequest(string title, int durationMinutes, decimal cost)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new InvalidOperationException("Le titre de la maintenance est obligatoire");
            if (durationMinutes <= 0)
                throw new InvalidOperationException("La durée doit être positive");
            if (cost < 0)
                throw new InvalidOperationException("Le coût ne peut pas être négatif");
        }
    }
}
