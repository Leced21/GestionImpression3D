using Backend.Data;
using Backend.Helpers;
using Backend.Hubs;
using Backend.Interface;
using Backend.Mappers;
using Backend.Middleware;
using Backend.Options;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuration des services de base ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Évite les boucles infinies de sérialisation JSON si vos entités ont des relations bidirectionnelles
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Récupération et validation stricte de la clé JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("La clé JWT ('Jwt:Key') est manquante dans la configuration ou est trop courte (minimum 32 caractères).");
}

var clientPortalAudience = builder.Configuration.GetValue("ClientPortal:JwtAudience", "PrintFlow3DClientPortal");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Optionnel : Élimine la tolérance par défaut de 5 minutes pour l'expiration du token
        };
    })
    // Schéma distinct pour le portail client externe : audience différente de celle des
    // Users internes, afin qu'un token client ne puisse jamais être accepté par un endpoint
    // interne (et inversement), même si les deux partagent la même clé de signature.
    .AddJwtBearer("ClientPortal", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = clientPortalAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- 1bis. Rate limiting (garde-fou global + protection brute-force sur l'authentification) ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    var globalPermitLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 200);
    var globalWindowSeconds = builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
    var authPermitLimit = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 5);
    var authWindowSeconds = builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60);

    // Garde-fou global par IP, appliqué à toute l'API.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = globalPermitLimit,
            Window = TimeSpan.FromSeconds(globalWindowSeconds),
            QueueLimit = 0
        });
    });

    // Politique plus stricte pour les endpoints d'authentification (login/register/refresh),
    // afin de limiter le brute-force sur les identifiants et les refresh tokens.
    options.AddPolicy("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authPermitLimit,
            Window = TimeSpan.FromSeconds(authWindowSeconds),
            QueueLimit = 0
        });
    });
});

// --- 2. Configuration de la base de données ---
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
});

// --- 3. Documentation & Outils ---
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));

// --- 4. Injection de dépendances (Scope & Business Logic) ---
// Métier : Pièces et Commerciaux
builder.Services.AddScoped<IPieceRepository, PieceRepository>();
builder.Services.AddScoped<IPieceService, PieceService>();
builder.Services.AddScoped<ICommercialRepository, CommercialRepository>();
builder.Services.AddScoped<ICommercialService, CommercialService>();
builder.Services.AddScoped<IPrinterRepository, PrinterRepository>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IPrintJobRepository, PrintJobRepository>();
builder.Services.AddScoped<IPrintJobService, PrintJobService>();
builder.Services.AddScoped<IMaterialStockRepository, MaterialStockRepository>();
builder.Services.AddScoped<IMaterialStockService, MaterialStockService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IExcelExportRepository, ExcelExportRepository>();
builder.Services.AddScoped<IPieceVersionRepository, PieceVersionRepository>();
builder.Services.AddScoped<IPieceVersionService, PieceVersionService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationManagerService, NotificationManagerService>();
builder.Services.AddScoped<IOrdreFabricationRepository, OrdreFabricationRepository>();
builder.Services.AddScoped<IOrdreFabricationService, OrdreFabricationService>();
builder.Services.AddScoped<IPrintProfileRepository, PrintProfileRepository>();
builder.Services.AddScoped<IPrintProfileService, PrintProfileService>();
builder.Services.AddScoped<ISTLAnalyzerService, STLAnalyzerService>();

// Métier : Projets et Exports
builder.Services.AddScoped<IProjetRepository, ProjetRepository>();
builder.Services.AddScoped<IProjetService, ProjetService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();

// Métier : Utilisateurs (Auth, Audit, Users)
// Note : IAuthRepository a été retiré conformément au nettoyage de l'architecture.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMaterialConsumptionRepository, MaterialConsumptionRepository>();
builder.Services.AddScoped<IPrinterMaintenanceRepository, PrinterMaintenanceRepository>();
builder.Services.AddScoped<IMaterialConsumptionService, MaterialConsumptionService>();
builder.Services.AddScoped<IPrinterMaintenanceService, PrinterMaintenanceService>();
builder.Services.AddScoped<IPrintIncidentRepository, PrintIncidentRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IDevisRepository, DevisRepository>();
builder.Services.AddScoped<IPrintIncidentService, PrintIncidentService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDevisService, DevisService>();
builder.Services.AddScoped<IFactureRepository, FactureRepository>();
builder.Services.AddScoped<IFactureService, FactureService>();
builder.Services.AddScoped<IClientMagicLinkRepository, ClientMagicLinkRepository>();
builder.Services.AddScoped<IClientPortalAuthService, ClientPortalAuthService>();
// Sans SMTP configuré (dev/local), on logge les emails au lieu de tenter un vrai envoi.
if (string.IsNullOrWhiteSpace(builder.Configuration[$"{SmtpOptions.SectionName}:Host"]))
{
    builder.Services.AddTransient<IEmailSender, LoggingEmailSender>();
}
else
{
    builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
}
builder.Services.AddScoped<IClientPortalMailSender, ClientPortalMailSender>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();

// Mappers & Validations
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.Services.AddScoped<IUserValidator, UserValidator>();

// --- 5. Configuration de la stratégie CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost",
            "https://localhost",
            "http://localhost:4200",
            "https://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               // Essentiel pour que le frontend Angular puisse lire le nom du fichier PDF généré
               .WithExposedHeaders("Content-Disposition");
    });
});
builder.Logging.ClearProviders();

builder.Logging.AddConsole();

builder.Logging.AddDebug();

builder.Services.AddResponseCompression();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- 6. Pipeline des Middlewares (L'ordre est TRÈS important) ---

// Étape A : Gestion des reverse proxies (doit être tout en haut)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseResponseCompression();
// Étape B : Gestion du CORS (Placé haut pour intercepter et autoriser immédiatement les requêtes OPTIONS)
app.UseCors("AllowAngular");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Étape C : Sécurité et Routage (L'authentification TOUJOURS avant l'autorisation)
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

// Les migrations sont une opération de déploiement. Elles ne sont appliquées au
// démarrage que lorsqu'elles sont explicitement activées (développement local,
// conteneur d'initialisation, etc.).
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Attente de la base de données SQL Server...");
        await EnsureDatabaseAsync(app.Configuration, logger);
        logger.LogInformation("Application des migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Base de données prête.");

        await SeedData.InitializeAsync(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration failed during startup");
        throw;
    }
}
app.MapGet("/", () => Results.Ok("3D Inspire API"));

static async Task EnsureDatabaseAsync(IConfiguration configuration, ILogger logger, int maxRetries = 20, TimeSpan? delay = null)
{
    delay ??= TimeSpan.FromSeconds(5);
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("La chaîne de connexion par défaut est manquante.");
    }

    var builder = new SqlConnectionStringBuilder(connectionString)
    {
        ConnectTimeout = 5
    };

    if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
    {
        throw new InvalidOperationException("La base de données cible est absente dans la chaîne de connexion.");
    }

    var databaseName = builder.InitialCatalog;
    var masterBuilder = new SqlConnectionStringBuilder(connectionString)
    {
        InitialCatalog = "master",
        ConnectTimeout = 5
    };

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
            await masterConnection.OpenAsync();

            await using var checkCommand = masterConnection.CreateCommand();
            checkCommand.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
            checkCommand.Parameters.AddWithValue("@name", databaseName);
            var exists = await checkCommand.ExecuteScalarAsync();

            if (exists == null)
            {
                logger.LogInformation("La base de données {Database} n'existe pas. Création en cours...", databaseName);
                var safeDatabaseName = databaseName.Replace("]", "]]" );
                await using var createCommand = masterConnection.CreateCommand();
                createCommand.CommandText = $"CREATE DATABASE [{safeDatabaseName}]";
                await createCommand.ExecuteNonQueryAsync();
                logger.LogInformation("Base de données {Database} créée.", databaseName);
            }

            await using var targetConnection = new SqlConnection(builder.ConnectionString);
            await targetConnection.OpenAsync();
            logger.LogInformation("SQL Server est prêt après {Attempt} tentative(s).", attempt);
            return;
        }
        catch (SqlException ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Impossible de se connecter à SQL Server (tentative {Attempt}/{Max}). Nouvelle tentative dans {DelaySeconds}s...", attempt, maxRetries, delay.Value.TotalSeconds);
            await Task.Delay(delay.Value);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Impossible de se connecter à SQL Server après {Attempt} tentatives.", attempt);
            throw;
        }
    }

    throw new InvalidOperationException("Impossible de se connecter à SQL Server après plusieurs tentatives.");
}

app.MapGet("/version", () =>
{
    return Results.Ok(new
    {
        Version = typeof(Program).Assembly.GetName().Version?.ToString(),
        Environment = app.Environment.EnvironmentName
    });
});
app.MapHealthChecks("/health");
app.Run();
