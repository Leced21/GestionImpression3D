using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            // Ajouter les permissions par défaut
            if (!context.Permissions.Any())
            {
                var permissions = new[]
                {
                // Pièces
                new Permission { Name = "pieces.view", Description = "Voir les pièces", Category = "Pièces" },
                new Permission { Name = "pieces.create", Description = "Créer des pièces", Category = "Pièces" },
                new Permission { Name = "pieces.edit", Description = "Modifier des pièces", Category = "Pièces" },
                new Permission { Name = "pieces.delete", Description = "Supprimer des pièces", Category = "Pièces" },

                // Projets
                new Permission { Name = "projects.view", Description = "Voir les projets", Category = "Projets" },
                new Permission { Name = "projects.create", Description = "Créer des projets", Category = "Projets" },
                new Permission { Name = "projects.edit", Description = "Modifier des projets", Category = "Projets" },
                new Permission { Name = "projects.delete", Description = "Supprimer des projets", Category = "Projets" },

                // Production
                new Permission { Name = "production.view", Description = "Voir la production", Category = "Production" },
                new Permission { Name = "production.start", Description = "Démarrer des impressions", Category = "Production" },
                new Permission { Name = "production.pause", Description = "Mettre en pause", Category = "Production" },
                new Permission { Name = "production.complete", Description = "Terminer des impressions", Category = "Production" },

                // Commercial
                new Permission { Name = "commercial.view", Description = "Voir le catalogue", Category = "Commercial" },
                new Permission { Name = "commercial.order", Description = "Passer des commandes", Category = "Commercial" },
                new Permission { Name = "commercial.manage", Description = "Gérer les commandes", Category = "Commercial" },

                // Stock
                new Permission { Name = "stock.view", Description = "Voir les stocks", Category = "Stock" },
                new Permission { Name = "stock.manage", Description = "Gérer les stocks", Category = "Stock" },

                // Admin
                new Permission { Name = "admin.users", Description = "Gérer les utilisateurs", Category = "Administration" },
                new Permission { Name = "admin.roles", Description = "Gérer les rôles", Category = "Administration" },
                new Permission { Name = "admin.audit", Description = "Voir les logs", Category = "Administration" }
            };

                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }

            // Ajouter les permissions par rôle
            if (!context.RolePermissions.Any())
            {
                var permissions = context.Permissions.ToList();
                var rolePermissions = new List<RolePermission>();

                // Admin: toutes les permissions
                foreach (var p in permissions)
                {
                    rolePermissions.Add(new RolePermission { Role = "Admin", PermissionId = p.Id });
                }

                // ProductionManager
                var prodManagerPermissions = permissions.Where(p =>
                    p.Name.StartsWith("pieces.") ||
                    p.Name.StartsWith("projects.") ||
                    p.Name.StartsWith("production.") ||
                    p.Name == "stock.view");
                foreach (var p in prodManagerPermissions)
                {
                    rolePermissions.Add(new RolePermission { Role = "ProductionManager", PermissionId = p.Id });
                }

                // Operator
                var operatorPermissions = permissions.Where(p =>
                    p.Name == "production.start" ||
                    p.Name == "production.pause" ||
                    p.Name == "production.complete" ||
                    p.Name == "pieces.view");
                foreach (var p in operatorPermissions)
                {
                    rolePermissions.Add(new RolePermission { Role = "Operator", PermissionId = p.Id });
                }

                // Commercial
                var commercialPermissions = permissions.Where(p =>
                    p.Name.StartsWith("commercial.") ||
                    p.Name == "pieces.view");
                foreach (var p in commercialPermissions)
                {
                    rolePermissions.Add(new RolePermission { Role = "Commercial", PermissionId = p.Id });
                }

                // Viewer
                var viewerPermissions = permissions.Where(p =>
                    p.Name == "pieces.view" ||
                    p.Name == "projects.view" ||
                    p.Name == "production.view");
                foreach (var p in viewerPermissions)
                {
                    rolePermissions.Add(new RolePermission { Role = "Viewer", PermissionId = p.Id });
                }

                context.RolePermissions.AddRange(rolePermissions);
                await context.SaveChangesAsync();
            }

            // Admin par défaut : uniquement sur une base vide, pour amorcer le tout premier
            // compte (l'inscription publique force toujours le rôle "User" et les invitations
            // nécessitent déjà un Admin existant, donc sans ça personne ne peut jamais devenir Admin).
            if (!context.Users.Any())
            {
                var email = configuration["AdminSeed:Email"];
                var password = configuration["AdminSeed:Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning(
                        "Aucun utilisateur en base et AdminSeed:Email/AdminSeed:Password non configurés : " +
                        "aucun compte Admin par défaut n'a été créé.");
                }
                else
                {
                    var admin = new User
                    {
                        Email = email.Trim(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                        Nom = configuration["AdminSeed:Nom"]?.Trim() is { Length: > 0 } nom ? nom : "Admin",
                        Prenom = configuration["AdminSeed:Prenom"]?.Trim() is { Length: > 0 } prenom ? prenom : "System",
                        Role = "Admin",
                        IsActive = true,
                        DateCreation = DateTime.UtcNow
                    };

                    context.Users.Add(admin);
                    await context.SaveChangesAsync();

                    logger.LogWarning(
                        "Compte Admin par défaut créé ({Email}). Change son mot de passe dès la première connexion.",
                        admin.Email);
                }
            }
        }
    }
}
