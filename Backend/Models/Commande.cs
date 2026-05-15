namespace Backend.Models
{
    public class Commande
    {
        public int Id { get; set; }
        public string NumeroCommande { get; set; } = string.Empty;
        public string ClientNom { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientTelephone { get; set; } = string.Empty;
        public string AdresseLivraison { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Statut { get; set; } = "En attente";
        public DateTime DateCommande { get; set; }
        public DateTime? DateLivraison { get; set; }
        public string? Notes { get; set; }
        public List<CommandeLigne> Lignes { get; set; } = new();
    }
}
