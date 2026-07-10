namespace Backend.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConsumedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
