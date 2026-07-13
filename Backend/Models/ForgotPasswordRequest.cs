using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
        [StringLength(100, ErrorMessage = "L'e-mail ne peut pas dépasser 100 caractères.")]
        public string Email { get; set; } = string.Empty;
    }
}
