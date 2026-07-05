using Backend.DTOs;
using Backend.Models;

namespace Backend.Interface
{
    public interface IUserMapper
    {
        UserDto ToDto(User user);
        User ToEntity(CreateUserRequest request);
        IEnumerable<UserDto> ToDtoList(IEnumerable<User> users);
    }
}
