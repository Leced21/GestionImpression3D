using Backend.Data;
using Backend.Enums;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TestProject
{
    public class AdminAuditLogsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token = string.Empty;
        private int _adminId;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AdminAuditLogsControllerTests(CustomWebApplicationFactory factory)
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
            if (admin != null)
            {
                _adminId = admin.Id;
                return;
            }

            admin = new User
            {
                Email = "admin@printflow3d.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Nom = "Admin",
                Prenom = "System",
                Role = "Admin",
                IsActive = true,
                DateCreation = DateTime.UtcNow
            };
            dbContext.Users.Add(admin);
            dbContext.SaveChanges();
            _adminId = admin.Id;
        }

        private async Task InitializeToken()
        {
            var loginRequest = new { email = "admin@printflow3d.com", password = "Admin123!" };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode)
                Assert.Fail("Impossible de s'authentifier pour le test");

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponse>(json, JsonOptions);
            _token = result?.Token ?? string.Empty;

            if (string.IsNullOrEmpty(_token))
                Assert.Fail("Le token d'authentification du test est vide");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        private void SeedAuditLog(AuditLog log)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.AuditLogs.Add(log);
            dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetAuditLogs_WithLimit_ReturnsAtMostLimitEntries()
        {
            for (var i = 0; i < 5; i++)
            {
                SeedAuditLog(AuditLog.CreateCreation(EntityType.Devis, 1000 + i, $"Devis-{i}", _adminId, "admin@printflow3d.com"));
            }

            var response = await _client.GetAsync("/api/admin/audit-logs?limit=2");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var logs = JsonSerializer.Deserialize<List<AuditLogResponse>>(json, JsonOptions);
            Assert.NotNull(logs);
            Assert.True(logs!.Count <= 2);
        }

        [Fact]
        public async Task GetAuditLogs_WithUserId_ReturnsOnlyThatUsersLogs()
        {
            SeedAuditLog(AuditLog.CreateCreation(EntityType.Facture, 2001, "Facture-A", _adminId, "admin@printflow3d.com"));
            SeedAuditLog(AuditLog.CreateCreation(EntityType.Facture, 2002, "Facture-B", 999999, "someone-else@printflow3d.com"));

            var response = await _client.GetAsync($"/api/admin/audit-logs?userId={_adminId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var logs = JsonSerializer.Deserialize<List<AuditLogResponse>>(json, JsonOptions);
            Assert.NotNull(logs);
            Assert.All(logs!, l => Assert.Equal(_adminId, l.UserId));
        }

        [Fact]
        public async Task GetAuditLogs_WithDateRange_ReturnsOnlyLogsInRange()
        {
            var response = await _client.GetAsync(
                $"/api/admin/audit-logs?startDate={DateTime.UtcNow.AddDays(-1):o}&endDate={DateTime.UtcNow.AddDays(1):o}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAuditLogs_WithEntityIdAlone_ReturnsBadRequest()
        {
            var response = await _client.GetAsync("/api/admin/audit-logs?entityId=1");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class AuditLogResponse
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserEmail { get; set; }
    }
}
