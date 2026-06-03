using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        public PermissionService(IPermissionRepository permissionRepository, IUserRepository userRepository)
        {
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
        }
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return permissions.ToList();
        }

        public async Task<List<RolePermission>> GetRolePermissionsAsync(string role)
        {
            var rolePermissions = await _permissionRepository.GetRolePermissionsAsync(role);
            return rolePermissions.ToList();
        }

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return new List<string>();

            var permissions = await _permissionRepository.GetUserPermissionsAsync(userId, user.Role);
            return permissions.ToList();
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            return await _permissionRepository.HasPermissionAsync(user.Role, permission);
        }
    }
}
