using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    // Ajout d'un index unique sur l'e-mail au niveau de la table pour des requêtes ultra-rapides
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key] // Optionnel car EF Core détecte automatiquement "Id" comme clé primaire
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)] // Limite la taille et permet l'indexation efficace
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)] // Un hash (BCrypt, Argon2, PBKDF2) a une longueur prévisible
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)] // Évite le nvarchar(max) pour un simple rôle (Admin, User, Viewer)
        public string Role { get; set; } = "User";

        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow; // Standardisation UTC

        public bool IsActive { get; set; } = true;
        // Refresh token fields for long-lived refresh
        [MaxLength(512)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }
    }
}