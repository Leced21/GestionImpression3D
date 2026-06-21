using Backend.Data;
using Backend.Helpers;
using Backend.Hubs;
using Backend.Interface;
using Backend.Mappers;
using Backend.Middleware;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    });

builder.Services.AddAuthorization();

// --- 2. Configuration de la base de données ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. Documentation & Outils ---
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

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
        policy.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               // Essentiel pour que le frontend Angular puisse lire le nom du fichier PDF généré
               .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- 6. Pipeline des Middlewares (L'ordre est TRÈS important) ---

// Étape A : Gestion des reverse proxies (doit être tout en haut)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Étape B : Gestion du CORS (Placé haut pour intercepter et autoriser immédiatement les requêtes OPTIONS)
app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Étape C : Sécurité et Routage (L'authentification TOUJOURS avant l'autorisation)
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
        throw;
    }
}

app.Run();
