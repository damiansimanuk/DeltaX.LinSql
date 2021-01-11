namespace DeltaX.RestApiDemo1.Dtos
{
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public CreateUsersRolesDto[] AddRoles { get; set; }
        public RemoveUsersRolesDto[] RemoveRoles { get; set; }
        public string Image { get; set; }
        public bool? Active { get; set; }
    }
}
