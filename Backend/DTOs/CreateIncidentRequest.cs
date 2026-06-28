using Backend.Enums;

namespace Backend.DTOs
{
    public class CreateIncidentRequest
    {
        public int? PrintJobId { get; set; }
        public int? PrinterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; } = IncidentSeverity.Moyenne;
    }
}
