using Backend.Interface;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository; // Utilisation directe du UserRepository
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IAuthMailSender _mailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IConfiguration configuration,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IAuthMailSender mailSender,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _passwordResetTokenRepository = passwordResetTokenRepository ?? throw new ArgumentNullException(nameof(passwordResetTokenRepository));
            _mailSender = mailSender ?? throw new ArgumentNullException(nameof(mailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email)) return null;

            // 1. Récupération de l'utilisateur via le Repository dédié aux données utilisateur
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // 2. Logique métier : Vérification des identifiants et du statut
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            if (!user.IsActive)
                return null;

            // 3. Logique métier : Génération de la session / Token
            var expirationTime = DateTime.UtcNow.AddHours(24);
            var token = GenerateJwtToken(user, expirationTime);

            // generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = user.Role,
                Token = token,
                Expiration = expirationTime,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            if (request == null) return null;

            // 1. Vérification des doublons
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null) return null;

            // 2. Logique métier : Le rôle est STRICTEMENT "User" pour tout le monde au départ
            var targetRole = "User";

            var user = new User
            {
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nom = request.Nom.Trim(),
                Prenom = request.Prenom.Trim(),
                Role = targetRole, // Sera toujours "User"
                IsActive = true,
                DateCreation = DateTime.UtcNow
            };

            // 3. Persistance
            await _userRepository.CreateAsync(user);

            var expirationTime = DateTime.UtcNow.AddHours(24);
            var token = GenerateJwtToken(user, expirationTime);
            // generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = user.Role,
                Token = token,
                Expiration = expirationTime,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse?> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null) return null;
            if (!user.RefreshTokenExpiry.HasValue || user.RefreshTokenExpiry.Value < DateTime.UtcNow) return null;

            var expirationTime = DateTime.UtcNow.AddHours(24);
            var token = GenerateJwtToken(user, expirationTime);

            // rotate refresh token
            var newRefresh = GenerateRefreshToken();
            user.RefreshToken = newRefresh;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = user.Role,
                Token = token,
                Expiration = expirationTime,
                RefreshToken = newRefresh
            };
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        // Déplacé depuis le Repository vers le Service (Sa place légitime)
        private string GenerateJwtToken(User user, DateTime expiresAt)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new InvalidOperationException("La clé de chiffrement JWT est manquante ou trop courte.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            return Convert.ToBase64String(random);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            // Ne jamais révéler si l'email correspond à un compte : même comportement
            // (aucune erreur) qu'il existe ou non, pour éviter l'énumération.
            var user = await _userRepository.GetByEmailAsync(email.Trim());
            if (user == null || !user.IsActive) return;

            var rawToken = GenerateRawResetToken();
            var tokenHash = HashResetToken(rawToken);

            await _passwordResetTokenRepository.CreateAsync(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetConfigInt("PasswordReset:LinkExpiryMinutes", 30)),
                CreatedAt = DateTime.UtcNow
            });

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
            var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

            // Ne jamais laisser un échec d'envoi remonter : la réponse de cet endpoint doit
            // rester identique que le compte existe ou non, y compris quand le fournisseur
            // mail est en panne (sinon un compte existant renverrait une erreur là où un
            // email inconnu renvoie normalement, ce qui recrée un canal d'énumération).
            try
            {
                await _mailSender.SendPasswordResetAsync(user.Email, user.Prenom, resetUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Échec de l'envoi de l'email de réinitialisation à l'utilisateur {UserId}", user.Id);
            }
        }

        public async Task<bool> ResetPasswordAsync(string rawToken, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(rawToken) || string.IsNullOrWhiteSpace(newPassword)) return false;

            var tokenHash = HashResetToken(rawToken);
            var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash);

            if (resetToken == null) return false;
            if (resetToken.ConsumedAt.HasValue) return false;
            if (resetToken.ExpiresAt < DateTime.UtcNow) return false;

            var user = resetToken.User;
            if (!user.IsActive) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // Révoque la session existante : un reset de mot de passe doit invalider les
            // connexions déjà actives (notamment si le compte était compromis).
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateAsync(user);

            resetToken.ConsumedAt = DateTime.UtcNow;
            await _passwordResetTokenRepository.UpdateAsync(resetToken);

            return true;
        }

        private int GetConfigInt(string key, int defaultValue)
        {
            var raw = _configuration[key];
            return int.TryParse(raw, out var value) ? value : defaultValue;
        }

        private static string GenerateRawResetToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string HashResetToken(string rawToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(hash);
        }
    }
}