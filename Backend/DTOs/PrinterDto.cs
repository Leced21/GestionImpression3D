namespace Backend.DTOs
{
    public class PrinterDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int MaxPrintSizeX { get; set; }
        public int MaxPrintSizeY { get; set; }
        public int MaxPrintSizeZ { get; set; }
        public int TotalPrintHours { get; set; }
        public int TotalPrintJobs { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public DateTime? LastPrint { get; set; }
        public bool IsActive { get; set; }
        public string StatusLabel => GetStatusLabel();
        public string StatusColor => GetStatusColor();

        private string GetStatusLabel() => Status switch
        {
            "Available" => "Disponible",
            "Printing" => "En impression",
            "Maintenance" => "Maintenance",
            "Offline" => "Hors ligne",
            "Error" => "Erreur",
            _ => "Inconnu"
        };

        private string GetStatusColor() => Status switch
        {
            "Available" => "#10b981",
            "Printing" => "#3b82f6",
            "Maintenance" => "#f59e0b",
            "Offline" => "#64748b",
            "Error" => "#ef4444",
            _ => "#64748b"
        };
    }

    public class CreatePrinterRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int MaxPrintSizeX { get; set; }
        public int MaxPrintSizeY { get; set; }
        public int MaxPrintSizeZ { get; set; }
    }

    public class UpdatePrinterRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

