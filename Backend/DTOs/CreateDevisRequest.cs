namespace Backend.DTOs
{
    public class CreateDevisRequest
    {
        public int ClientId { get; set; }
        public int? ProjetId { get; set; }
        public DateTime DateValidite { get; set; }
        public decimal TVA { get; set; } = 20;
        public string? Notes { get; set; }
        public string? Conditions { get; set; }
        public List<DevisLigneRequest> Lignes { get; set; } = new();
    }
}
