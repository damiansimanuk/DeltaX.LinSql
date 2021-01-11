using DeltaX.RestApiDemo1.Dtos;
using DeltaX.RestApiDemo1.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository repository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository repository, ILogger<UserController> logger)
        {
            this.repository = repository;
            _logger = logger;
        }

        [HttpGet("user/{id}")]
        public Task<UserDto> GetUser(int id)
        {
            return repository.GetUserAsync(id);
        }

        [HttpPost("users")]
        public Task<UserDto> PostUser([FromBody] CreateUserDto user)
        {
            return repository.InsertUserAsync(user);
        }

        [HttpGet("users")]
        public Task<IEnumerable<UserListDto>> GetUsersAsync()
        {
            return repository.GetUsersAsync();
        }

        [HttpPut("user/{userId}")]
        public Task<UserDto> UpdateUser(int userId, [FromBody] UpdateUserDto user)
        {
            return repository.UpdateUserAsync(userId, user);
        }

        [HttpDelete("user/{id}")]
        public Task<int> DeleteUser(int id)
        {
            return repository.RemoveUserAsync(id);
        }
    }
}
