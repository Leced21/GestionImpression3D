using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsRepository _settingsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogger _auditLogger;
        public UserSettingsService(IUserSettingsRepository settingsRepository,
        IUserRepository userRepository,
        IAuditLogger auditLogger)
        {
            _settingsRepository = settingsRepository;
            _userRepository = userRepository;
            _auditLogger = auditLogger;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur non trouvé");

            // Vérifier le mot de passe actuel
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new InvalidOperationException("Mot de passe actuel incorrect");

            // Vérifier la longueur du nouveau mot de passe
            if (request.NewPassword.Length < 6)
                throw new InvalidOperationException("Le nouveau mot de passe doit contenir au moins 6 caractères");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            await _auditLogger.LogUpdateAsync(EntityType.User, userId, "Password", "Modifié", "Mot de passe changé");

            return true;
        }

        public async Task<SettingsDto> GetSettingsAsync(int userId)
        {
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings == null)
            {
                // Créer des paramètres par défaut
                var defaultSettings = new UserSettings
                {
                    UserId = userId,
                    Language = "fr",
                    Timezone = "Europe/Paris",
                    DateFormat = "DD/MM/YYYY",
                    Theme = "light",
                    PrimaryColor = "#3b82f6",
                    EmailNotifications = true,
                    StockAlerts = true,
                    ProductionAlerts = true,
                    WeeklyReports = false,
                    TwoFactorEnabled = false
                };

                settings = await _settingsRepository.CreateAsync(defaultSettings);
            }

            return MapToDto(settings);
        }

        public async Task<bool> Toggle2FAAsync(int userId)
        {
            var settings = await _settingsRepository.GetByUserIdAsync(userId);
            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                settings.TwoFactorEnabled = true;
                await _settingsRepository.CreateAsync(settings);
            }
            else
            {
                settings.TwoFactorEnabled = !settings.TwoFactorEnabled;
                await _settingsRepository.UpdateAsync(settings);
            }

            await _auditLogger.LogUpdateAsync(EntityType.User, userId, "2FA",
                settings.TwoFactorEnabled ? "Désactivé" : "Activé",
                settings.TwoFactorEnabled ? "Activé" : "Désactivé");

            return settings.TwoFactorEnabled;
        }

        public async Task<User> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Utilisateur non trouvé");

            var oldNom = user.Nom;
            var oldPrenom = user.Prenom;

            user.Nom = request.Nom;
            user.Prenom = request.Prenom;

            var updated = await _userRepository.UpdateAsync(user);

            await _auditLogger.LogUpdateAsync(EntityType.User, userId, "Profile",
                $"Nom: {oldNom}, Prénom: {oldPrenom}",
                $"Nom: {request.Nom}, Prénom: {request.Prenom}");

            return updated;
        }

        public async Task<SettingsDto> UpdateSettingsAsync(int userId, SettingsDto settings)
        {
            var setting = await _settingsRepository.GetByUserIdAsync(userId);

            if (setting == null)
            {
                setting = new UserSettings { UserId = userId };
            }

            setting.Language = settings.Language;
            setting.Timezone = settings.Timezone;
            setting.DateFormat = settings.DateFormat;
            setting.Theme = settings.Theme;
            setting.PrimaryColor = settings.PrimaryColor;
            setting.EmailNotifications = settings.EmailNotifications;
            setting.StockAlerts = settings.StockAlerts;
            setting.ProductionAlerts = settings.ProductionAlerts;
            setting.WeeklyReports = settings.WeeklyReports;
            setting.TwoFactorEnabled = settings.TwoFactorEnabled;

            var updated = setting.Id == 0
                ? await _settingsRepository.CreateAsync(setting)
                : await _settingsRepository.UpdateAsync(setting);

            await _auditLogger.LogUpdateAsync(EntityType.User, userId, "Settings", "Modifié", "Paramètres mis à jour");

            return MapToDto(updated);
        }
        private static SettingsDto MapToDto(UserSettings settings)
        {
            return new SettingsDto
            {
                Language = settings.Language,
                Timezone = settings.Timezone,
                DateFormat = settings.DateFormat,
                Theme = settings.Theme,
                PrimaryColor = settings.PrimaryColor,
                EmailNotifications = settings.EmailNotifications,
                StockAlerts = settings.StockAlerts,
                ProductionAlerts = settings.ProductionAlerts,
                WeeklyReports = settings.WeeklyReports,
                TwoFactorEnabled = settings.TwoFactorEnabled
            };
        }
    }
}
