namespace Backend.DTOs
{
    public class SettingsDto
    {
        public string Language { get; set; } = "fr";
        public string Timezone { get; set; } = "Europe/Paris";
        public string DateFormat { get; set; } = "DD/MM/YYYY";
        public string Theme { get; set; } = "light";
        public string PrimaryColor { get; set; } = "#3b82f6";
        public bool EmailNotifications { get; set; } = true;
        public bool StockAlerts { get; set; } = true;
        public bool ProductionAlerts { get; set; } = true;
        public bool WeeklyReports { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
    }
}
