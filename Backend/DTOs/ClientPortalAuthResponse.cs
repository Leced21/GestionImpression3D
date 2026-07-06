namespace Backend.DTOs
{
    public class ClientPortalAuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public int ClientId { get; set; }
        public string ClientNom { get; set; } = string.Empty;
    }
}
