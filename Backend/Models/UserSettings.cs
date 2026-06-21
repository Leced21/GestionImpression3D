namespace Backend.Models
{
    public class UserSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Général
        public string Language { get; set; } = "fr";
        public string Timezone { get; set; } = "Europe/Paris";
        public string DateFormat { get; set; } = "DD/MM/YYYY";

        // Apparence
        public string Theme { get; set; } = "light";
        public string PrimaryColor { get; set; } = "#3b82f6";

        // Notifications
        public bool EmailNotifications { get; set; } = true;
        public bool StockAlerts { get; set; } = true;
        public bool ProductionAlerts { get; set; } = true;
        public bool WeeklyReports { get; set; } = false;

        // Sécurité
        public bool TwoFactorEnabled { get; set; } = false;

        // Métadonnées
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
