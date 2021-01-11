using DeltaX.RestApiDemo1.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1.Repository
{
    public interface IUserRepository
    {
        Task<UserDto> GetUserAsync(int id);
        Task<IEnumerable<UserListDto>> GetUsersAsync();
        Task<UserDto> InsertUserAsync(CreateUserDto item);
        Task<int> RemoveUserAsync(int id);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto item);
    }
}
