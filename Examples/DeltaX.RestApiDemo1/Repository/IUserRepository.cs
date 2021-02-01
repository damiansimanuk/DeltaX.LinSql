using DeltaX.RestApiDemo1.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1.Repository
{
    public interface IUserRepository
    {
        Task<UserModel> GetUserAsync(int id);
        Task<IEnumerable<UserListDto>> GetUsersAsync();
        Task<UserModel> InsertUserAsync(CreateUserDto item);
        Task<int> RemoveUserAsync(int id);
        Task<UserModel> UpdateUserAsync(int userId, UpdateUserDto item);
    }
}
