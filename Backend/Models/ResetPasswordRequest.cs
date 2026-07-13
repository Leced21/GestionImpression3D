using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Le token est obligatoire.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Le mot de passe doit contenir entre 8 et 100 caractères.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
