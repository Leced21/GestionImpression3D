namespace Backend.Models
{
    public class ClientMagicLink
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConsumedAt { get; set; }

        // Navigation
        public Client Client { get; set; } = null!;
    }
}
