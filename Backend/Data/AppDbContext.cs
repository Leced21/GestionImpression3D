using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Piece> Pieces { get; set; }
        public DbSet<DashboardStat> DashboardStats { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<CommandeLigne> CommandeLignes { get; set; }
        public DbSet<Projet> Projets { get; set; }
        public DbSet<ProjetPiece> ProjetPieces { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<MaterialStock> MaterialStocks { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<PieceVersion> PieceVersions { get; set; }
        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<OrdreFabrication> OrdresFabrication { get; set; }
        public DbSet<PrintProfile> PrintProfiles { get; set; }
        public DbSet<STLMetadata> STLMetadata { get; set; }
        public DbSet<MaterialConsumption> MaterialConsumptions { get; set; }
        public DbSet<PrinterMaintenance> PrinterMaintenances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Piece>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Reference).HasMaxLength(50);
                entity.Property(e => e.Categorie).HasMaxLength(50);
                entity.Property(e => e.Materiau).HasMaxLength(50);

                // 💡 FIX CATALOGUE : On force l'enum PieceStatus à s'enregistrer en texte
                entity.Property(e => e.Statut)
                      .HasConversion<string>()
                      .HasMaxLength(30);

                // Ignorer les propriétés calculées
                entity.Ignore(e => e.CoutTotal);
                entity.Ignore(e => e.Marge);
                entity.Ignore(e => e.MargePourcentage);
            });

            modelBuilder.Entity<Commande>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroCommande).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ClientNom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ClientEmail).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Statut).HasMaxLength(50);
            });

            modelBuilder.Entity<CommandeLigne>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 💡 FIX CLÉ ÉTRANGÈRE FANTÔME : On lie explicitement la navigation inverse
                entity.HasOne(e => e.Commande)
                      .WithMany(c => c.Lignes) // Assure-toi d'avoir 'public List<CommandeLigne> CommandeLignes { get; set; }' dans Commande.cs
                      .HasForeignKey(e => e.CommandeId);

                entity.HasOne(e => e.Piece)
                      .WithMany()
                      .HasForeignKey(e => e.PieceId);
            });

            modelBuilder.Entity<Projet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Reference).HasMaxLength(50);

                // 💡 FIX PROJET STATUT : Si c'est un enum, on le convertit en string aussi
                entity.Property(e => e.Statut)
                      .HasConversion<string>()
                      .HasMaxLength(50);
            });

            modelBuilder.Entity<ProjetPiece>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Projet)
                      .WithMany(p => p.ProjetPieces)
                      .HasForeignKey(e => e.ProjetId);
                entity.HasOne(e => e.Piece)
                      .WithMany(p => p.ProjetPieces)
                      .HasForeignKey(e => e.PieceId);

                entity.HasIndex(e => new { e.ProjetId, e.PieceId }).IsUnique();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserEmail).HasMaxLength(200);
                entity.Property(e => e.EntityName).HasMaxLength(200);
                entity.Property(e => e.FieldName).HasMaxLength(100);
                entity.Property(e => e.OldValue).HasMaxLength(500);
                entity.Property(e => e.NewValue).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Printer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Reference).HasMaxLength(50);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.ApiKey).HasMaxLength(200);
            });

            modelBuilder.Entity<PrintJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GCodeFileName).HasMaxLength(200);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.JobNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                // 🚀 Sécurité anti-cycle validée !
                entity.HasOne(e => e.OrdreFabrication)
                      .WithMany(o => o.PrintJobs)
                      .HasForeignKey(e => e.OrdreFabricationId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<MaterialStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Reference).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Supplier).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Quantity);
            });

            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Token).IsUnique();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Permission).WithMany().HasForeignKey(e => e.PermissionId);
            });

            modelBuilder.Entity<PieceVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ChangeLog).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.HasOne(e => e.Piece)
                      .WithMany()
                      .HasForeignKey(e => e.PieceId);

                entity.HasIndex(e => new { e.PieceId, e.VersionNumber }).IsUnique();
            });

            modelBuilder.Entity<AppNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.ReferenceType).HasMaxLength(50);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<OrdreFabrication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Reference).IsRequired().HasMaxLength(20);

                // Enums convertis en string pour la lisibilité en BDD
                entity.Property(e => e.Statut).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.Priorite).HasConversion<string>().HasMaxLength(20);

                entity.HasIndex(e => e.Reference).IsUnique();

                // 💡 FIX CASCADE MULTIPLE : L'Ordre de Fabrication est lié à la fois à une Pièce et un Projet.
                // On désactive la cascade ici pour éviter que la suppression d'un Projet ou d'une Pièce 
                // ne crée des conflits de suppression sur les PrintJobs rattachés.
                entity.HasOne(e => e.Piece)
                      .WithMany()
                      .HasForeignKey(e => e.PieceId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Projet)
                      .WithMany()
                      .HasForeignKey(e => e.ProjetId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<PrintProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Materiau).HasMaxLength(50);
            });

            modelBuilder.Entity<STLMetadata>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(255);
                entity.Property(e => e.Errors).HasMaxLength(500);
                entity.HasIndex(e => e.PieceId).IsUnique();
            });
            modelBuilder.Entity<MaterialConsumption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Unit).HasMaxLength(10);
                entity.Property(e => e.Type).HasMaxLength(30);
                entity.Property(e => e.Reason).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.MaterialStock)
                      .WithMany()
                      .HasForeignKey(e => e.MaterialStockId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PrintJob)
                      .WithMany()
                      .HasForeignKey(e => e.PrintJobId);

                entity.HasOne(e => e.OrdreFabrication)
                      .WithMany()
                      .HasForeignKey(e => e.OrdreFabricationId);

                entity.HasOne(e => e.ConsumedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ConsumedBy)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.ConsumedAt);
                entity.HasIndex(e => e.Type);
            });

            modelBuilder.Entity<PrinterMaintenance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).HasMaxLength(30);
                entity.Property(e => e.Title).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(30);
                entity.Property(e => e.PerformedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Printer)
                      .WithMany()
                      .HasForeignKey(e => e.PrinterId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Status);
            });

            // 🚀 BUCKET MAGIQUE : Configure globalement tous les types decimal à (18,2)
            // Éteint instantanément les 25 avertissements de troncature de tes logs !
            var decimalProperties = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalProperties)
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}
