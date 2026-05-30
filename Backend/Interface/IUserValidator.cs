using Backend.DTOs;

namespace Backend.Interface
{
    public interface IUserValidator
    {
        void ValidateCreate(CreateUserRequest request);
        void ValidateRole(string role);
    }
}
