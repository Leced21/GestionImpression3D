using Backend.Interface;
using Backend.Models;

namespace Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
        public Task<User?> GetUserByIdAsync(int id)
        {
            return _authRepository.GetUserByIdAsync(id);
        }

        public Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            return _authRepository.LoginAsync(request);
        }

        public Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            return _authRepository.RegisterAsync(request);
        }

        public Task<bool> UserExistsAsync(string email)
        {
            return _authRepository.UserExistsAsync(email);
        }
    }
}
