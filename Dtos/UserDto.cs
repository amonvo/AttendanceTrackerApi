namespace AttendanceTrackerApi.Dtos
{
    /// <summary>
    /// Výstupní DTO pro uživatele – nikdy neposílá hash hesla!
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
