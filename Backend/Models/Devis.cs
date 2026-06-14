using Backend.Enums;

namespace Backend.Models
{
    public class Devis
    {
        public int Id { get; set; }
        public string NumeroDevis { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public int? ProjetId { get; set; }
        public DateTime DateEmission { get; set; }
        public DateTime DateValidite { get; set; }
        public decimal TotalHT { get; set; }
        public decimal TVA { get; set; } = 20;
        public decimal TotalTTC { get; set; }
        public DevisStatus Statut { get; set; } = DevisStatus.Brouillon; // Brouillon, Envoyé, Accepté, Refusé, Expiré
        public string? Notes { get; set; }
        public string? Conditions { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Client Client { get; set; } = null!;
        public Projet? Projet { get; set; }
        public List<DevisLigne> Lignes { get; set; } = new();
    }
}
