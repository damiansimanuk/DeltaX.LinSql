using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeltaX.RestApiDemo1.Dtos
{

    public class UserModel
    {
        public int Id { get; internal set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public UsersRolesModel[] Roles { get; set; }
        public string Image { get; set; }
        public bool Active { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
