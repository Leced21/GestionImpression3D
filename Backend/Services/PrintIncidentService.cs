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
            ValidateIncidentRequest(request);

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

        public async Task<PrintIncident?> UpdateAsync(int id, CreateIncidentRequest request)
        {
            ValidateIncidentRequest(request);

            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return null;
            if (incident.Status == IncidentStatus.Résolu || incident.Status == IncidentStatus.Fermé)
                throw new InvalidOperationException("Un incident résolu ou fermé ne peut plus être modifié.");

            incident.PrintJobId = request.PrintJobId;
            incident.PrinterId = request.PrinterId;
            incident.Title = request.Title.Trim();
            incident.Description = request.Description.Trim();
            incident.Severity = request.Severity;

            var updated = await _incidentRepository.UpdateAsync(incident);
            await _auditLogger.LogUpdateAsync(EntityType.PrintIncident, id, "Incident", "Modifié", updated.Title);

            return updated;
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

            if (incident.Status == IncidentStatus.Résolu || incident.Status == IncidentStatus.Fermé)
                throw new InvalidOperationException("Cet incident est déjà résolu ou fermé.");
            if (string.IsNullOrWhiteSpace(request.Resolution))
                throw new InvalidOperationException("La résolution est obligatoire.");

            if (!_currentUser.UserId.HasValue)
                throw new InvalidOperationException("Impossible de résoudre l'incident sans utilisateur authentifié.");

            var resolved = await _incidentRepository.ResolveAsync(id, request.Resolution, _currentUser.UserId.Value);

            await _auditLogger.LogStatusChangeAsync(EntityType.PrintIncident, id, incident.Status.ToString(), "Résolu");

            return resolved;
        }

        public async Task<PrintIncident?> UpdateStatusAsync(int id, IncidentStatus status)
        {
            var incident = await _incidentRepository.GetByIdAsync(id);
            if (incident == null) return null;

            if (!IsValidTransition(incident.Status, status))
                throw new InvalidOperationException($"Transition impossible de {incident.Status} vers {status}");

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

        private static void ValidateIncidentRequest(CreateIncidentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new InvalidOperationException("Le titre de l'incident est obligatoire.");
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new InvalidOperationException("La description de l'incident est obligatoire.");
            if (!request.PrintJobId.HasValue && !request.PrinterId.HasValue)
                throw new InvalidOperationException("Un incident doit être lié à une impression ou à une imprimante.");

            request.Title = request.Title.Trim();
            request.Description = request.Description.Trim();
        }

        private static bool IsValidTransition(IncidentStatus oldStatus, IncidentStatus newStatus)
        {
            if (oldStatus == newStatus) return true;

            return oldStatus switch
            {
                IncidentStatus.Ouvert => newStatus is IncidentStatus.EnCours or IncidentStatus.Résolu,
                IncidentStatus.EnCours => newStatus is IncidentStatus.Résolu or IncidentStatus.Fermé,
                IncidentStatus.Résolu => newStatus == IncidentStatus.Fermé,
                IncidentStatus.Fermé => false,
                _ => false
            };
        }
    }
}
