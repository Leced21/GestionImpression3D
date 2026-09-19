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
        public decimal PowerWatts { get; private set; } = 120m;
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
            int maxSizeZ,
            decimal powerWatts = 120m)
        {
            if (powerWatts <= 0)
                throw new ArgumentException("La puissance de l'imprimante doit être supérieure à 0 W.", nameof(powerWatts));

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
                PowerWatts = powerWatts,
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

        public void UpdateDetails(
            string nom,
            string reference,
            string model,
            string brand,
            PrinterType type,
            string ipAddress,
            int maxSizeX,
            int maxSizeY,
            int maxSizeZ,
            decimal powerWatts,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom de l'imprimante est obligatoire.", nameof(nom));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Le modèle de l'imprimante est obligatoire.", nameof(model));
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("La marque de l'imprimante est obligatoire.", nameof(brand));
            if (maxSizeX <= 0 || maxSizeY <= 0 || maxSizeZ <= 0)
                throw new ArgumentException("Le volume d'impression doit être supérieur à 0 mm sur les trois axes.");
            if (powerWatts <= 0)
                throw new ArgumentException("La puissance de l'imprimante doit être supérieure à 0 W.", nameof(powerWatts));

            Nom = nom.Trim();
            Reference = reference?.Trim() ?? string.Empty;
            Model = model.Trim();
            Brand = brand.Trim();
            Type = type;
            IpAddress = ipAddress?.Trim() ?? string.Empty;
            MaxPrintSizeX = maxSizeX;
            MaxPrintSizeY = maxSizeY;
            MaxPrintSizeZ = maxSizeZ;
            PowerWatts = powerWatts;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
