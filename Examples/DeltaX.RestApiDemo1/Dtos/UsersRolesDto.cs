using System;

namespace DeltaX.RestApiDemo1.Dtos
{
     
    public class UsersRolesDto
    {
        public int UserId { get; internal set; }
        public int RolId { get; internal set; }        
        public string RolName { get; set; }
        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
