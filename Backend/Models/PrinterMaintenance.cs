using Backend.Enums;

namespace Backend.Models
{
    public class PrinterMaintenance
    {
        public int Id { get; set; }
        public int PrinterId { get; set; }
        public MaintenanceType Type { get; set; } = MaintenanceType.Preventive;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
        public int DurationMinutes { get; set; }
        public decimal Cost { get; set; }
        public string? PerformedBy { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Printer Printer { get; set; } = null!;
    }
}
