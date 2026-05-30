namespace Backend.DTOs
{
    public class CreatePrintJobRequest
    {
        public int PieceId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Priority { get; set; } = "Normal";
        public int EstimatedDurationMinutes { get; set; }
        public decimal EstimatedMaterialGrams { get; set; }
        public string? Notes { get; set; }
    }
}
