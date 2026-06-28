using Backend.DTOs;
using Backend.Interface;

namespace Backend.Helpers
{
    public class UserValidator : IUserValidator
    {
        public void ValidateCreate(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("L'email est requis");

            if (!UserValidators.IsValidEmail(request.Email))
                throw new ArgumentException("Email invalide");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Le mot de passe est requis");

            if (!UserValidators.IsValidPassword(request.Password))
                throw new ArgumentException("Le mot de passe doit contenir au moins 6 caractères");

            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("Le nom est requis");

            if (string.IsNullOrWhiteSpace(request.Prenom))
                throw new ArgumentException("Le prénom est requis");
        }

        public void ValidateRole(string role)
        {
            if (!UserValidators.IsValidRole(role))
                throw new ArgumentException($"Rôle invalide. Valeurs possibles: {string.Join(", ", UserValidators.GetValidRoles())}");
        }
    }
}
