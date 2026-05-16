namespace Backend.Models
{
    public class CommandeRequest
    {
        public string ClientNom { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientTelephone { get; set; } = string.Empty;
        public string AdresseLivraison { get; set; } = string.Empty;
        public List<CommandeItem> Items { get; set; } = new List<CommandeItem>();
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public DateTime DateCommande { get; set; } = DateTime.Now;
    }
}
