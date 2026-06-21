using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IUserSettingsService
    {
        Task<SettingsDto> GetSettingsAsync(int userId);
        Task<SettingsDto> UpdateSettingsAsync(int userId, SettingsDto settings);
        Task<User> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<bool> Toggle2FAAsync(int userId);
    }
}
