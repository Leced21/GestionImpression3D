using Backend.Models;

namespace Backend.Interface
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<List<RolePermission>> GetRolePermissionsAsync(string role);
    }
}
