namespace Backend.DTOs
{
    public class PrintJobDto
    {
        public int Id { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public int PieceId { get; set; }
        public string PieceName { get; set; } = string.Empty;
        public string PieceReference { get; set; } = string.Empty;
        public int? PrinterId { get; set; }
        public string? PrinterName { get; set; }
        public int? OperatorId { get; set; }
        public string? OperatorName { get; set; }
        public int Quantity { get; set; }
        public int QuantityCompleted { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public int? ActualDurationMinutes { get; set; }
        public decimal EstimatedMaterialGrams { get; set; }
        public decimal ActualMaterialGrams { get; set; }
        public string? FailureReason { get; set; }
        public string? Notes { get; set; }
        public int ProgressPercent { get; set; }
        public string StatusLabel => GetStatusLabel();
        public string StatusColor => GetStatusColor();
        public string PriorityLabel => GetPriorityLabel();

        private string GetStatusLabel() => Status switch
        {
            "Pending" => "En attente",
            "Queued" => "En file",
            "Printing" => "En impression",
            "Paused" => "En pause",
            "Completed" => "Terminé",
            "Failed" => "Échoué",
            "Cancelled" => "Annulé",
            _ => "Inconnu"
        };

        private string GetStatusColor() => Status switch
        {
            "Pending" => "#f59e0b",
            "Queued" => "#64748b",
            "Printing" => "#3b82f6",
            "Paused" => "#8b5cf6",
            "Completed" => "#10b981",
            "Failed" => "#ef4444",
            "Cancelled" => "#6b7280",
            _ => "#64748b"
        };

        private string GetPriorityLabel() => Priority switch
        {
            "Low" => "Basse",
            "Normal" => "Normale",
            "High" => "Haute",
            "Urgent" => "Urgente",
            _ => "Normale"
        };
    }
}
