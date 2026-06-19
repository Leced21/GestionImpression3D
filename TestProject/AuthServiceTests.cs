using Backend.Interface;
using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace TestProject
{
    public class AuthServiceTests
    {
        private readonly IAuthService _authService;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configMock;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configMock = new Mock<IConfiguration>();

            // Configuration JWT
            _configMock.Setup(c => c["Jwt:Key"]).Returns("c6f0b8d2e5a47f39d1e8c2a9f7b4d6e8");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("PrintFlow3D");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("PrintFlow3DUsers");

            _authService = new AuthService(_userRepositoryMock.Object, _configMock.Object);
        }
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var password = "Admin123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                PasswordHash = hashedPassword,
                Nom = "Admin",
                Prenom = "System",
                Role = "Admin",
                IsActive = true
            };

            var request = new LoginRequest { Email = "admin@test.com", Password = "Admin123!" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal("admin@test.com", result.Email);
            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                PasswordHash = hashedPassword,
                IsActive = true
            };

            var request = new LoginRequest { Email = "admin@test.com", Password = "wrongpassword" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var request = new LoginRequest { Email = "nonexistent@test.com", Password = "Test123!" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_WithInactiveUser_ReturnsNull()
        {
            // Arrange
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var user = new User
            {
                Id = 1,
                Email = "inactive@test.com",
                PasswordHash = hashedPassword,
                IsActive = false
            };

            var request = new LoginRequest { Email = "inactive@test.com", Password = "Admin123!" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Register_WithNewUser_CreatesAccount()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@test.com",
                Password = "Test123!",
                Nom = "Test",
                Prenom = "User"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.Id = 1; return u; });

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.Email);
            Assert.Equal("User", result.Role);
            Assert.NotNull(result.Token);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsNull()
        {
            // Arrange
            var existingUser = new User { Id = 1, Email = "existing@test.com", PasswordHash = "hash" };
            var request = new RegisterRequest { Email = "existing@test.com", Password = "Test123!", Nom = "Test", Prenom = "User" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.Null(result);
        }
    }
}
