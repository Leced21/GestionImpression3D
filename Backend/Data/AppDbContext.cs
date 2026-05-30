using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext: DbContext
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
                entity.HasOne(e => e.Commande)
                      .WithMany()
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
                entity.Property(e => e.Statut).HasMaxLength(50);
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

                // Unicité ProjetId + PieceId pour éviter les doublons
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
            });
        }
    }
}
