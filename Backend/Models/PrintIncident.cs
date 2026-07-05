using Backend.Enums;

namespace Backend.Models
{
    public class PrintIncident
    {
        public int Id { get; set; }
        public int? PrintJobId { get; set; }
        public int? PrinterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Moyenne; // Critique, Haute, Moyenne, Basse
        public IncidentStatus Status { get; set; } = IncidentStatus.Ouvert; // Ouvert, En cours, Résolu, Fermé
        public DateTime OccurredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public int? ReportedBy { get; set; }
        public int? ResolvedBy { get; set; }
        public string? Attachments { get; set; }

        // Navigation
        public PrintJob? PrintJob { get; set; }
        public Printer? Printer { get; set; }
        public User? ReportedByUser { get; set; }
        public User? ResolvedByUser { get; set; }
    }
}
