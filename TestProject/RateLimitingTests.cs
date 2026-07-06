using Backend.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Text;
using System.Text.Json;

namespace TestProject
{
    // Factory dédiée avec une limite d'authentification volontairement basse, pour vérifier
    // le comportement 429 réel sans perturber CustomWebApplicationFactory (partagée par les
    // autres classes de tests d'intégration, qui ont besoin de se logger librement).
    public sealed class RateLimitingTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"GestionImpression3D_RateLimitingTests_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", "integration-tests-only-secret-key-32-characters");
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
            builder.UseSetting("RateLimiting:Auth:PermitLimit", "3");
            builder.UseSetting("RateLimiting:Auth:WindowSeconds", "60");
            builder.UseSetting("RateLimiting:Global:PermitLimit", "100000");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.RemoveAll<AppDbContext>();

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    public class RateLimitingTests : IClassFixture<RateLimitingTestFactory>
    {
        private readonly HttpClient _client;

        public RateLimitingTests(RateLimitingTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        // Un seul test couvre les deux bornes (dans la limite / au-delà) : les deux classes de
        // requêtes partagent le même compteur de fenêtre glissante (même partition IP), donc les
        // séparer en deux [Fact] sur la même factory les rendrait dépendants de l'ordre d'exécution.
        [Fact]
        public async Task Login_ExceedingAuthPermitLimit_ReturnsTooManyRequestsOnlyAfterLimitReached()
        {
            var badLogin = new { email = "nobody@nowhere.test", password = "wrong-password" };
            var payload = JsonSerializer.Serialize(badLogin);

            for (var attempt = 0; attempt < 3; attempt++)
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("/api/auth/login", content);
                Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
            }

            var overLimitContent = new StringContent(payload, Encoding.UTF8, "application/json");
            var overLimitResponse = await _client.PostAsync("/api/auth/login", overLimitContent);

            Assert.Equal(HttpStatusCode.TooManyRequests, overLimitResponse.StatusCode);
        }
    }
}
