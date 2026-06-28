namespace Backend.DTOs
{
    public class UpdatePrintJobStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public int? ActualDurationMinutes { get; set; }
        public decimal? ActualMaterialGrams { get; set; }
        public string? FailureReason { get; set; }
    }
}
