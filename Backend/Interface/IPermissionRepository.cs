using Backend.Models;

namespace Backend.Interface
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetByCategoryAsync(string category);
        Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(string role);
        Task<bool> HasPermissionAsync(string role, string permission);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, string userRole);
    }
}
