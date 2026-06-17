using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PrintIncidentService : IPrintIncidentService
    {
        private readonly IPrintIncidentRepository _incidentRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogger _auditLogger;
        public PrintIncidentService(IPrintIncidentRepository incidentRepository, ICurrentUserService currentUser, IAuditLogger auditLogger)
        {
            _incidentRepository = incidentRepository;
            _currentUser = currentUser;
            _auditLogger = auditLogger;
        }
        public async Task<PrintIncident> CreateAsync(CreateIncidentRequest request)
        {
            var incident = new PrintIncident
            {
                PrintJobId = request.PrintJobId,
                PrinterId = request.PrinterId,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Status = IncidentStatus.Ouvert,
                OccurredAt = DateTime.UtcNow,
                ReportedBy = _currentUser.UserId
            };

            var created = await _incidentRepository.CreateAsync(incident);

            await _auditLogger.LogCreationAsync(EntityType.PrintIncident, created.Id, created.Title);

            return created;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return false;

            var result = await _incidentRepository.DeleteAsync(id);

            if (result)
                await _auditLogger.LogDeletionAsync(EntityType.PrintIncident, id, incident.Title);

            return result;
        }

        public async Task<IEnumerable<PrintIncident>> GetAllAsync()
        {
            return await _incidentRepository.GetAllAsync();
        }

        public async Task<PrintIncident?> GetByIdAsync(int id)
        {
            return await _incidentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PrintIncident>> GetByPrinterAsync(int printerId)
        {
            return await _incidentRepository.GetByPrinterAsync(printerId);
        }

        public async Task<IEnumerable<PrintIncident>> GetByPrintJobAsync(int printJobId)
        {
            return await _incidentRepository.GetByPrintJobAsync(printJobId);
        }

        public async Task<IncidentStatisticsDto> GetStatisticsAsync(DateTime? start = null, DateTime? end = null)
        {
            return await _incidentRepository.GetStatisticsAsync(start, end);
        }

        public async Task<PrintIncident?> ResolveAsync(int id, ResolveIncidentRequest request)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return null;

            var resolved = await _incidentRepository.ResolveAsync(id, request.Resolution, _currentUser.UserId.Value);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintIncident, id, incident.Status.ToString(), "Résolu");

            return resolved;
        }

        public async Task<PrintIncident?> UpdateStatusAsync(int id, IncidentStatus status)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return null;

            var oldStatus = incident.Status;
            incident.Status = status;

            if (status == IncidentStatus.EnCours && oldStatus == IncidentStatus.Ouvert)
            {
                // Logique supplémentaire si nécessaire
            }

            var updated = await _incidentRepository.UpdateAsync(incident);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintIncident, id, oldStatus.ToString(), status.ToString());

            return updated;
        }
    }
}
