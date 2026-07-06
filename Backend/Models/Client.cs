namespace Backend.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string CodePostal { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
        public string Pays { get; set; } = "France";
        public string? Siret { get; set; }
        public string? TVAIntra { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public List<Devis> Devis { get; set; } = new();
        public List<Commande> Commandes { get; set; } = new();
        public List<Facture> Factures { get; set; } = new();
    }
}
