using Backend.Enums;

namespace Backend.Models
{
    public class Printer
    {
        public int Id { get; private set; }
        public string Nom { get; private set; } = string.Empty;
        public string Reference { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public string Brand { get; private set; } = string.Empty;
        public PrinterType Type { get; private set; }
        public PrinterStatus Status { get; private set; }
        public string IpAddress { get; private set; } = string.Empty;
        public string ApiKey { get; private set; } = string.Empty;
        public int MaxPrintSizeX { get; private set; }  // mm
        public int MaxPrintSizeY { get; private set; }  // mm
        public int MaxPrintSizeZ { get; private set; }  // mm
        public int TotalPrintHours { get; private set; }
        public int TotalPrintJobs { get; private set; }
        public DateTime? LastMaintenance { get; private set; }
        public DateTime? LastPrint { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Printer() { }

        public static Printer Create(
            string nom,
            string reference,
            string model,
            string brand,
            PrinterType type,
            string ipAddress,
            int maxSizeX,
            int maxSizeY,
            int maxSizeZ)
        {
            return new Printer
            {
                Nom = nom,
                Reference = reference,
                Model = model,
                Brand = brand,
                Type = type,
                Status = PrinterStatus.Available,
                IpAddress = ipAddress,
                MaxPrintSizeX = maxSizeX,
                MaxPrintSizeY = maxSizeY,
                MaxPrintSizeZ = maxSizeZ,
                TotalPrintHours = 0,
                TotalPrintJobs = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateStatus(PrinterStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartPrint()
        {
            if (Status != PrinterStatus.Available)
                throw new InvalidOperationException($"Impossible de démarrer une impression. Statut actuel: {Status}");

            Status = PrinterStatus.Printing;
            TotalPrintJobs++;
            LastPrint = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompletePrint(int durationMinutes)
        {
            if (Status != PrinterStatus.Printing)
                throw new InvalidOperationException("Aucune impression en cours");

            Status = PrinterStatus.Available;
            TotalPrintHours += durationMinutes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void FailPrint(string? reason = null)
        {
            if (Status != PrinterStatus.Printing)
                throw new InvalidOperationException("Aucune impression en cours");

            Status = PrinterStatus.Error;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMaintenance()
        {
            Status = PrinterStatus.Maintenance;
            LastMaintenance = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvailable()
        {
            Status = PrinterStatus.Available;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateApiKey(string apiKey)
        {
            ApiKey = apiKey;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
