using Backend.Models;

namespace Backend.Interface
{
    public interface IAuthRepository
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UserExistsAsync(string email);
    }
}
