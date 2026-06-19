using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TestProject
{
    public class PrintJobControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private string _token = string.Empty;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PrintJobControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            EnsureAdminExistsInDatabase();
            InitializeToken().GetAwaiter().GetResult();
        }

        private void EnsureAdminExistsInDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated();

            var admin = dbContext.Users.FirstOrDefault(u => u.Email == "admin@printflow3d.com");
            if (admin != null) return;

            dbContext.Users.Add(new User
            {
                Email = "admin@printflow3d.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Nom = "Admin",
                Prenom = "System",
                Role = "Admin",
                IsActive = true,
                DateCreation = DateTime.UtcNow
            });
            dbContext.SaveChanges();
        }

        private async Task InitializeToken()
        {
            var loginRequest = new { email = "admin@printflow3d.com", password = "Admin123!" };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponse>(json, JsonOptions);
            _token = result?.Token ?? string.Empty;

            if (!string.IsNullOrEmpty(_token))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/printjob");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/printjob/1");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(request);

            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetStatistics_ReturnsOkResult()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/printjob/statistics");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
