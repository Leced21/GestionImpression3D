using Backend.Models;

namespace Backend.Interface
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);
        Task<PasswordResetToken> UpdateAsync(PasswordResetToken token);
    }
}
