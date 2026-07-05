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

        [Fact]
        public async Task Login_WithNullRequest_ReturnsNull()
        {
            var result = await _authService.LoginAsync(null!);

            Assert.Null(result);
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithEmptyEmail_ReturnsNull()
        {
            var request = new LoginRequest { Email = "", Password = "Test123!" };

            var result = await _authService.LoginAsync(request);

            Assert.Null(result);
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithValidCredentials_SetsRefreshTokenOnUser()
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
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

            var result = await _authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.RefreshToken == result.RefreshToken && u.RefreshTokenExpiry.HasValue)), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithValidToken_ReturnsNewTokenAndRotatesRefreshToken()
        {
            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                Nom = "Admin",
                Prenom = "System",
                Role = "Admin",
                RefreshToken = "old-refresh-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(1)
            };

            _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("old-refresh-token")).ReturnsAsync(user);

            var result = await _authService.RefreshAsync("old-refresh-token");

            Assert.NotNull(result);
            Assert.Equal("admin@test.com", result.Email);
            Assert.NotNull(result.Token);
            Assert.NotEqual("old-refresh-token", result.RefreshToken);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.RefreshToken == result.RefreshToken)), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WithExpiredToken_ReturnsNull()
        {
            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                RefreshToken = "expired-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)
            };

            _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("expired-token")).ReturnsAsync(user);

            var result = await _authService.RefreshAsync("expired-token");

            Assert.Null(result);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RefreshAsync_WithUnknownToken_ReturnsNull()
        {
            _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync("unknown-token")).ReturnsAsync((User?)null);

            var result = await _authService.RefreshAsync("unknown-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshAsync_WithNullOrEmptyToken_ReturnsNull()
        {
            Assert.Null(await _authService.RefreshAsync(""));
            Assert.Null(await _authService.RefreshAsync(null!));
            _userRepositoryMock.Verify(x => x.GetByRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UserExistsAsync_WithExistingEmail_ReturnsTrue()
        {
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("admin@test.com")).ReturnsAsync(new User { Id = 1, Email = "admin@test.com" });

            var result = await _authService.UserExistsAsync("admin@test.com");

            Assert.True(result);
        }

        [Fact]
        public async Task UserExistsAsync_WithUnknownEmail_ReturnsFalse()
        {
            _userRepositoryMock.Setup(x => x.GetByEmailAsync("nobody@test.com")).ReturnsAsync((User?)null);

            var result = await _authService.UserExistsAsync("nobody@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task UserExistsAsync_WithNullOrEmptyEmail_ReturnsFalse()
        {
            Assert.False(await _authService.UserExistsAsync(""));
            Assert.False(await _authService.UserExistsAsync(null!));
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
        {
            var user = new User { Id = 42, Email = "admin@test.com" };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(42)).ReturnsAsync(user);

            var result = await _authService.GetUserByIdAsync(42);

            Assert.NotNull(result);
            Assert.Equal(42, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
        {
            _userRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((User?)null);

            var result = await _authService.GetUserByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task LogoutAsync_WithExistingUser_ClearsRefreshTokenAndReturnsTrue()
        {
            var user = new User
            {
                Id = 1,
                Email = "admin@test.com",
                RefreshToken = "some-refresh-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(5)
            };
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            var result = await _authService.LogoutAsync(1);

            Assert.True(result);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == 1 && u.RefreshToken == null && u.RefreshTokenExpiry == null)), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_WithNonExistingUser_ReturnsFalse()
        {
            _userRepositoryMock.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((User?)null);

            var result = await _authService.LogoutAsync(99);

            Assert.False(result);
            _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}
