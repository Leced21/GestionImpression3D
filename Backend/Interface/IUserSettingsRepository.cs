using Backend.Models;

namespace Backend.Interface
{
    public interface IUserSettingsRepository
    {
        Task<UserSettings?> GetByUserIdAsync(int userId);
        Task<UserSettings> CreateAsync(UserSettings settings);
        Task<UserSettings> UpdateAsync(UserSettings settings);
        Task<bool> DeleteAsync(int userId);
    }
}
