using Backend.DTOs;
using Backend.Interface;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services
{
    public class ClientPortalAuthService : IClientPortalAuthService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientMagicLinkRepository _magicLinkRepository;
        private readonly IClientPortalMailSender _mailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClientPortalAuthService> _logger;

        public ClientPortalAuthService(
            IClientRepository clientRepository,
            IClientMagicLinkRepository magicLinkRepository,
            IClientPortalMailSender mailSender,
            IConfiguration configuration,
            ILogger<ClientPortalAuthService> logger)
        {
            _clientRepository = clientRepository;
            _magicLinkRepository = magicLinkRepository;
            _mailSender = mailSender;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task RequestAccessAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            // Ne jamais révéler si l'email correspond à un client : même comportement
            // (aucune erreur) que le client existe ou non, pour éviter l'énumération.
            var client = await _clientRepository.GetByEmailAsync(email.Trim());
            if (client == null) return;

            var rawToken = GenerateRawToken();
            var tokenHash = HashToken(rawToken);

            await _magicLinkRepository.CreateAsync(new ClientMagicLink
            {
                ClientId = client.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetConfigInt("ClientPortal:LinkExpiryMinutes", 30)),
                CreatedAt = DateTime.UtcNow
            });

            var baseUrl = _configuration["ClientPortal:FrontendBaseUrl"] ?? "http://localhost:4200";
            var magicLinkUrl = $"{baseUrl}/portail/acces?token={Uri.EscapeDataString(rawToken)}";

            // Ne jamais laisser un échec d'envoi remonter : la réponse de cet endpoint doit
            // rester identique que le client existe ou non, y compris quand le fournisseur
            // mail est en panne (sinon un client existant renverrait 500 là où un email
            // inconnu renvoie normalement, ce qui recrée un canal d'énumération).
            try
            {
                await _mailSender.SendMagicLinkAsync(client.Email, client.Nom, magicLinkUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec de l'envoi du lien magique au client {ClientId}", client.Id);
            }
        }

        public async Task<ClientPortalAuthResponse?> ConsumeAsync(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken)) return null;

            var tokenHash = HashToken(rawToken);
            var link = await _magicLinkRepository.GetByTokenHashAsync(tokenHash);

            if (link == null) return null;
            if (link.ConsumedAt.HasValue) return null;
            if (link.ExpiresAt < DateTime.UtcNow) return null;

            link.ConsumedAt = DateTime.UtcNow;
            await _magicLinkRepository.UpdateAsync(link);

            var expiration = DateTime.UtcNow.AddHours(GetConfigInt("ClientPortal:SessionExpiryHours", 2));
            var token = GenerateClientJwt(link.Client, expiration);

            return new ClientPortalAuthResponse
            {
                Token = token,
                Expiration = expiration,
                ClientId = link.Client.Id,
                ClientNom = link.Client.Nom
            };
        }

        private int GetConfigInt(string key, int defaultValue)
        {
            var raw = _configuration[key];
            return int.TryParse(raw, out var value) ? value : defaultValue;
        }

        private static string GenerateRawToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string HashToken(string rawToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hash);
        }

        private string GenerateClientJwt(Client client, DateTime expiresAt)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                throw new InvalidOperationException("La clé de chiffrement JWT est manquante ou trop courte.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Name, client.Nom)
            };

            var audience = _configuration["ClientPortal:JwtAudience"] ?? "PrintFlow3DClientPortal";

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
