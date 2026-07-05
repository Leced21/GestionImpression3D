using Backend.Enums;

namespace Backend.Models
{
    public class Facture
    {
        public int Id { get; set; }
        public string NumeroFacture { get; set; } = string.Empty;
        public int DevisId { get; set; }
        public int ClientId { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime DateEcheance { get; set; }
        public decimal TotalHT { get; set; }
        public decimal TVA { get; set; }
        public decimal TotalTTC { get; set; }
        public FactureStatus Statut { get; set; } = FactureStatus.Émise;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Devis Devis { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public List<FactureLigne> Lignes { get; set; } = new();
    }
}
