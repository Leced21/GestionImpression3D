namespace Backend.Models
{
    public class PrintProfile
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PrinterId { get; set; }
        public string Materiau { get; set; } = "PLA";
        public decimal NozzleTemp { get; set; }  // °C
        public decimal BedTemp { get; set; }     // °C
        public decimal LayerHeight { get; set; } // mm
        public decimal Speed { get; set; }       // mm/s
        public int Infill { get; set; }          // %
        public string InfillPattern { get; set; } = "Gyroid";
        public bool Supports { get; set; }
        public string SupportType { get; set; } = "Tree";
        public decimal MaterialMultiplier { get; set; } = 1.0m;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Printer Printer { get; set; } = null!;
    }
}
