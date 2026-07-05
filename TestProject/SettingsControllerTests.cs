using Backend.Data;
using Backend.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TestProject
{
    public class SettingsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token = string.Empty;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SettingsControllerTests(CustomWebApplicationFactory factory)
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
            if (!response.IsSuccessStatusCode)
                Assert.Fail("Impossible de s'authentifier pour le test");

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponse>(json, JsonOptions);
            _token = result?.Token ?? string.Empty;

            if (string.IsNullOrEmpty(_token))
                Assert.Fail("Le token d'authentification du test est vide");

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        [Fact]
        public async Task GetSettings_ReturnsDefaultSettingsOnFirstCall()
        {
            var response = await _client.GetAsync("/api/settings");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var settings = JsonSerializer.Deserialize<SettingsResponse>(json, JsonOptions);
            Assert.NotNull(settings);
            Assert.Equal("fr", settings!.Language);
        }

        [Fact]
        public async Task UpdateSettings_ThenGetSettings_ReturnsPersistedValues()
        {
            var update = new
            {
                Language = "en",
                Timezone = "Europe/London",
                DateFormat = "MM/DD/YYYY",
                Theme = "dark",
                PrimaryColor = "#10b981",
                EmailNotifications = false,
                StockAlerts = true,
                ProductionAlerts = false,
                WeeklyReports = true,
                TwoFactorEnabled = false
            };
            var content = new StringContent(JsonSerializer.Serialize(update), Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("/api/settings", content);
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            var getResponse = await _client.GetAsync("/api/settings");
            var json = await getResponse.Content.ReadAsStringAsync();
            var settings = JsonSerializer.Deserialize<SettingsResponse>(json, JsonOptions);

            Assert.NotNull(settings);
            Assert.Equal("en", settings!.Language);
            Assert.Equal("dark", settings.Theme);
        }

        [Fact]
        public async Task Toggle2FA_FlipsTwoFactorEnabled()
        {
            var firstToggle = await _client.PostAsync("/api/settings/toggle-2fa", null);
            Assert.Equal(HttpStatusCode.OK, firstToggle.StatusCode);
            var firstJson = await firstToggle.Content.ReadAsStringAsync();
            var firstResult = JsonSerializer.Deserialize<ToggleResponse>(firstJson, JsonOptions);

            var secondToggle = await _client.PostAsync("/api/settings/toggle-2fa", null);
            var secondJson = await secondToggle.Content.ReadAsStringAsync();
            var secondResult = JsonSerializer.Deserialize<ToggleResponse>(secondJson, JsonOptions);

            Assert.NotEqual(firstResult!.Enabled, secondResult!.Enabled);
        }

        [Fact]
        public async Task GetSystemInfo_AsAdmin_ReturnsOkWithInfo()
        {
            var response = await _client.GetAsync("/api/settings/system-info");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var info = JsonSerializer.Deserialize<SystemInfoResponse>(json, JsonOptions);
            Assert.NotNull(info);
            Assert.False(string.IsNullOrEmpty(info!.Environment));
            Assert.False(string.IsNullOrEmpty(info.DbStatus));
        }
    }

    public class SettingsResponse
    {
        public string Language { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
    }

    public class ToggleResponse
    {
        public bool Enabled { get; set; }
    }

    public class SystemInfoResponse
    {
        public string AppVersion { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string DbStatus { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
    }
}
