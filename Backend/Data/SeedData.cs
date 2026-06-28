using Backend.Models;

namespace Backend.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
        }
    }
}
