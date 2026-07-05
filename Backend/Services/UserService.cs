using Backend.DTOs;
using Backend.Enums;
using Backend.Interface;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserMapper _userMapper;
        private readonly IUserValidator _userValidator;
        private readonly IAuditLogger _auditLogger;

        public UserService(
            IUserRepository userRepository,
            IUserMapper userMapper,
            IUserValidator userValidator,
            IAuditLogger auditLogger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userMapper = userMapper ?? throw new ArgumentNullException(nameof(userMapper));
            _userValidator = userValidator ?? throw new ArgumentNullException(nameof(userValidator));
            _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validation des données d'entrée
            _userValidator.ValidateCreate(request);

            // Vérifier si l'email existe déjà
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Cet email est déjà utilisé");

            // Définir le rôle par défaut si non spécifié et le valider
            var role = string.IsNullOrEmpty(request.Role) ? "User" : request.Role.Trim();
            _userValidator.ValidateRole(role);

            // Créer l'entité utilisateur
            var user = _userMapper.ToEntity(request);
            user.Role = role;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.DateCreation = DateTime.UtcNow; // Cohérence avec le fuseau horaire standardisé

            var created = await _userRepository.CreateAsync(user);

            // Audit
            await _auditLogger.LogCreationAsync(EntityType.User, created.Id, created.Email);

            return _userMapper.ToDto(created);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            // Sécurité : Vérification pour le dernier admin
            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                    throw new InvalidOperationException("Impossible de supprimer le dernier administrateur de l'application.");
            }

            var result = await _userRepository.DeleteAsync(id);

            if (result)
            {
                await _auditLogger.LogDeletionAsync(EntityType.User, id, user.Email);
            }

            return result;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _userMapper.ToDtoList(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _userMapper.ToDto(user) : null;
        }

        public async Task<UserDto?> UpdateRoleAsync(int id, string role)
        {
            if (string.IsNullOrEmpty(role)) throw new ArgumentException("Le rôle ne peut pas être vide.");

            // Validation de la nomenclature du rôle
            _userValidator.ValidateRole(role);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var oldRole = user.Role;

            // Si le rôle ne change pas, on évite un traitement inutile et un log fantôme
            if (string.Equals(oldRole, role, StringComparison.Ordinal))
                return _userMapper.ToDto(user);

            // 🟢 FIX DE SÉCURITÉ CRITIQUE : Vérification du dernier admin lors d'une rétrogradation
            if (string.Equals(oldRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                {
                    throw new InvalidOperationException("Impossible de changer le rôle du dernier administrateur.");
                }
            }

            user.Role = role;
            var updated = await _userRepository.UpdateAsync(user);

            // Audit
            await _auditLogger.LogUpdateAsync(EntityType.User, id, "Role", oldRole, role);

            return _userMapper.ToDto(updated);
        }

        public async Task<UserDto?> UpdateStatusAsync(int id, bool isActive)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var oldStatus = user.IsActive;

            // Si le statut est déjà celui demandé, on s'arrête là
            if (oldStatus == isActive)
                return _userMapper.ToDto(user);

            // Sécurité : Vérification spéciale pour le dernier admin actif
            if (!isActive && string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var activeAdmins = await _userRepository.CountActiveAdminsAsync();
                if (activeAdmins <= 1)
                    throw new InvalidOperationException("Impossible de désactiver le dernier administrateur actif.");
            }

            user.IsActive = isActive;
            var updated = await _userRepository.UpdateAsync(user);

            // Audit
            await _auditLogger.LogUpdateAsync(EntityType.User, id, "IsActive", oldStatus.ToString(), isActive.ToString());

            return _userMapper.ToDto(updated);
        }
    }
}