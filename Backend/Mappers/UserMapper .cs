using Azure.Core;
using Backend.DTOs;
using Backend.Interface;
using Backend.Models;

namespace Backend.Mappers
{
    public class UserMapper : IUserMapper
    {
        public UserDto ToDto(User user)
        {
            if (user == null) return null!;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Role = user.Role,
                IsActive = user.IsActive,
                DateCreation = user.DateCreation
            };
        }

        public IEnumerable<UserDto> ToDtoList(IEnumerable<User> users)
        {
            return users.Select(ToDto);
        }

        public User ToEntity(CreateUserRequest request)
        {
            return new User
            {
                Email = request.Email,
                Nom = request.Nom,
                Prenom = request.Prenom,
                Role = request.Role ?? "User",
                IsActive = true,
                DateCreation = DateTime.UtcNow
            };
        }
    }
}
