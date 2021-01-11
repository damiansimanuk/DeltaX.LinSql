namespace DeltaX.RestApiDemo1.Dtos
{
    public class CreateUserDto
    { 
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public CreateUsersRolesDto[] Roles { get; set; }  
    }
}
