using Backend.Models;

namespace Backend.Interface
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UserExistsAsync(string email);
        Task<AuthResponse?> RefreshAsync(string refreshToken);
        Task<bool> LogoutAsync(int userId);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string rawToken, string newPassword);
    }
}
