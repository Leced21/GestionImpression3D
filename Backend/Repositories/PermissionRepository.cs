using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;
        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetByCategoryAsync(string category)
        {
            return await _context.Permissions
            .Where(p => p.Category == category)
            .ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<IEnumerable<RolePermission>> GetRolePermissionsAsync(string role)
        {
            return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.Role == role)
            .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId, string userRole)
        {
            var permissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.Role == userRole)
                .Select(rp => rp.Permission.Name)
                .ToListAsync();

            return permissions;
        }

        public async Task<bool> HasPermissionAsync(string role, string permission)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => rp.Role == role && rp.Permission.Name == permission);
        }
    }
}
