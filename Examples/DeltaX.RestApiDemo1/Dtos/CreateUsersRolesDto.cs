namespace DeltaX.RestApiDemo1.Dtos
{
    public class CreateUsersRolesDto
    { 
        public string RolName { get; set; }
        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; } 
    }
}
