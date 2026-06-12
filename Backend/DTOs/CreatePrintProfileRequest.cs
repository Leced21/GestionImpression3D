namespace Backend.DTOs
{
    public class CreatePrintProfileRequest
    {
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PrinterId { get; set; }
        public string Materiau { get; set; } = "PLA";
        public decimal NozzleTemp { get; set; } = 210;
        public decimal BedTemp { get; set; } = 60;
        public decimal LayerHeight { get; set; } = 0.20m;
        public decimal Speed { get; set; } = 60;
        public int Infill { get; set; } = 20;
        public string InfillPattern { get; set; } = "Gyroid";
        public bool Supports { get; set; }
        public string SupportType { get; set; } = "Tree";
        public decimal MaterialMultiplier { get; set; } = 1.0m;
        public bool IsDefault { get; set; }
    }
}
