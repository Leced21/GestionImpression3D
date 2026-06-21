using Backend.Data;
using Backend.Interface;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UserSettingsRepository:IUserSettingsRepository
    {
        private readonly AppDbContext _context;
        public UserSettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSettings> CreateAsync(UserSettings settings)
        {
            settings.CreatedAt = DateTime.UtcNow;
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var settings = await GetByUserIdAsync(userId);
            if (settings == null) return false;

            _context.UserSettings.Remove(settings);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserSettings?> GetByUserIdAsync(int userId)
        {
            return await _context.UserSettings
    .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<UserSettings> UpdateAsync(UserSettings settings)
        {
            settings.UpdatedAt = DateTime.UtcNow;
            _context.Entry(settings).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return settings;
        }
    }
}
