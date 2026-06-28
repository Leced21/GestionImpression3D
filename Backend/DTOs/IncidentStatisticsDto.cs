using Backend.Enums;

namespace Backend.DTOs
{
    public class IncidentStatisticsDto
    {
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int InProgressIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public int ClosedIncidents { get; set; }
        public Dictionary<IncidentSeverity, int> BySeverity { get; set; } = new();
        public Dictionary<string, int> ByPrinter { get; set; } = new();
        public double AverageResolutionTimeHours { get; set; }
    }
}
