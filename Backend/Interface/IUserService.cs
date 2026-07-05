using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto?> UpdateRoleAsync(int id, string role);
        Task<UserDto?> UpdateStatusAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}
