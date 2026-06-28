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
    public class PiecesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token = string.Empty;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PiecesControllerTests(CustomWebApplicationFactory factory)
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
        public async Task CreatePiece_WithValidData_ReturnsCreatedResult()
        {
            var newPiece = new
            {
                Nom = "Test Integration",
                Reference = $"TEST-INT-{DateTime.UtcNow.Ticks}",
                Description = "Test description"
            };

            var response = await SendJsonAsync(HttpMethod.Post, "/api/piece", newPiece);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdPiece = JsonSerializer.Deserialize<PieceResponse>(responseContent, JsonOptions);
            Assert.NotNull(createdPiece);
            Assert.Equal("Test Integration", createdPiece.Nom);
        }

        [Fact]
        public async Task GetAllPieces_ReturnsOkResult()
        {
            var request = CreateAuthorizedRequest(HttpMethod.Get, "/api/piece");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPieceById_WithValidId_ReturnsOkResult()
        {
            var piece = await CreatePieceAsync("Test Get By Id", $"TEST-GET-{DateTime.UtcNow.Ticks}");
            var request = CreateAuthorizedRequest(HttpMethod.Get, $"/api/piece/{piece.Id}");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePieceStatus_WithValidId_ReturnsOkResult()
        {
            var piece = await CreatePieceAsync("Test Status Update", $"TEST-STATUS-{DateTime.UtcNow.Ticks}");
            var request = CreateAuthorizedRequest(HttpMethod.Patch, $"/api/piece/{piece.Id}/statut");
            request.Content = new StringContent("\"Conception\"", Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeletePiece_WithValidId_ReturnsNoContent()
        {
            var piece = await CreatePieceAsync("Test Delete", $"TEST-DELETE-{DateTime.UtcNow.Ticks}");
            var request = CreateAuthorizedRequest(HttpMethod.Delete, $"/api/piece/{piece.Id}");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private async Task<PieceResponse> CreatePieceAsync(string nom, string reference)
        {
            var response = await SendJsonAsync(HttpMethod.Post, "/api/piece", new { Nom = nom, Reference = reference });
            if (response.StatusCode != HttpStatusCode.Created)
                Assert.Fail("Impossible de créer une pièce pour le test");

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdPiece = JsonSerializer.Deserialize<PieceResponse>(responseContent, JsonOptions);
            return createdPiece ?? throw new InvalidOperationException("Réponse de création de pièce invalide.");
        }

        private async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string url, object body)
        {
            var request = CreateAuthorizedRequest(method, url);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            return await _client.SendAsync(request);
        }

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return request;
        }
    }

    public class PieceResponse
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
    }
}
