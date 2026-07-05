namespace Backend.Options
{
    public sealed class SmtpOptions
    {
        public const string SectionName = "Mail:Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "PrintFlow3D";
        public bool EnableSsl { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
