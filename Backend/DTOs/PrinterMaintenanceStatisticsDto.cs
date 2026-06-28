namespace Backend.DTOs
{
    public class PrinterMaintenanceStatisticsDto
    {
        public int TotalMaintenances { get; set; }
        public int PreventiveCount { get; set; }
        public int CorrectiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageDurationMinutes { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public DateTime? NextScheduled { get; set; }
    }
}
