namespace Backend.DTOs
{
    public class CreateClientRequest
    {
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
    }
}
