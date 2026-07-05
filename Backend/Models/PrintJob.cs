using Backend.Enums;

namespace Backend.Models
{
    public class PrintJob
    {
        public int Id { get; private set; }
        public string JobNumber { get; private set; } = string.Empty;
        public int PieceId { get; private set; }
        public int? PrinterId { get; private set; }
        public int? OperatorId { get; private set; }
        public int Quantity { get; private set; }
        public int QuantityCompleted { get; private set; }
        public PrintJobStatus Status { get; private set; }
        public PrintJobPriority Priority { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public int? EstimatedDurationMinutes { get; private set; }
        public int? ActualDurationMinutes { get; private set; }
        public decimal EstimatedMaterialGrams { get; private set; }
        public decimal ActualMaterialGrams { get; private set; }
        public string? GCodeFileName { get; private set; }
        public string? FailureReason { get; private set; }
        public string? Notes { get; private set; }
        public int? OrdreFabricationId { get; set; }

        // Navigation properties
        public Piece Piece { get; private set; } = null!;
        public Printer? Printer { get; private set; }
        public User? Operator { get; private set; }
        public OrdreFabrication? OrdreFabrication { get; set; }

        private PrintJob() { }

        public static PrintJob Create(
            int pieceId,
            int quantity,
            PrintJobPriority priority,
            int estimatedDurationMinutes,
            decimal estimatedMaterialGrams,
            string? notes = null)
        {
            return new PrintJob
            {
                JobNumber = GenerateJobNumber(),
                PieceId = pieceId,
                Quantity = quantity,
                QuantityCompleted = 0,
                Status = PrintJobStatus.Pending,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                EstimatedDurationMinutes = estimatedDurationMinutes,
                EstimatedMaterialGrams = estimatedMaterialGrams,
                Notes = notes
            };
        }

        public void AssignToPrinter(int printerId, int? operatorId = null)
        {
            if (Status != PrintJobStatus.Pending && Status != PrintJobStatus.Queued)
                throw new InvalidOperationException($"Impossible d'assigner une imprimante. Statut actuel: {Status}");

            PrinterId = printerId;
            OperatorId = operatorId;
            Status = PrintJobStatus.Queued;
        }

        public void Start()
        {
            if (Status != PrintJobStatus.Queued)
                throw new InvalidOperationException($"Impossible de démarrer le job. Statut actuel: {Status}");

            Status = PrintJobStatus.Printing;
            StartedAt = DateTime.UtcNow;
        }

        public void Pause()
        {
            if (Status != PrintJobStatus.Printing)
                throw new InvalidOperationException($"Impossible de mettre en pause. Statut actuel: {Status}");

            Status = PrintJobStatus.Paused;
        }

        public void Resume()
        {
            if (Status != PrintJobStatus.Paused)
                throw new InvalidOperationException($"Impossible de reprendre. Statut actuel: {Status}");

            Status = PrintJobStatus.Printing;
        }

        public void Complete(int actualDurationMinutes, decimal actualMaterialGrams)
        {
            if (Status != PrintJobStatus.Printing && Status != PrintJobStatus.Queued)
                throw new InvalidOperationException($"Impossible de terminer le job. Statut actuel: {Status}");

            Status = PrintJobStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ActualDurationMinutes = actualDurationMinutes;
            ActualMaterialGrams = actualMaterialGrams;
            QuantityCompleted = Quantity;
        }

        public void Fail(string reason)
        {
            if (Status != PrintJobStatus.Printing && Status != PrintJobStatus.Queued)
                throw new InvalidOperationException($"Impossible de marquer comme échoué. Statut actuel: {Status}");

            Status = PrintJobStatus.Failed;
            FailureReason = reason;
            CompletedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == PrintJobStatus.Completed || Status == PrintJobStatus.Failed)
                throw new InvalidOperationException($"Impossible d'annuler un job terminé ou échoué");

            Status = PrintJobStatus.Cancelled;
            CompletedAt = DateTime.UtcNow;
        }

        public void UpdateProgress(int completedCount)
        {
            if (completedCount > Quantity)
                throw new ArgumentException("La quantité complétée ne peut pas dépasser la quantité totale");

            QuantityCompleted = completedCount;
        }

        private static string GenerateJobNumber()
        {
            return $"JOB-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 10000)}";
        }
    }
}
