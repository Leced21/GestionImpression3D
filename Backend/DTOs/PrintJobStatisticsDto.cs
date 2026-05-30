namespace Backend.DTOs
{
    public class PrintJobStatisticsDto
    {
        public int TotalJobs { get; set; }
        public int PendingJobs { get; set; }
        public int QueuedJobs { get; set; }
        public int PrintingJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int FailedJobs { get; set; }
        public int TotalDurationMinutes { get; set; }
        public decimal TotalMaterialGrams { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
