using System.ComponentModel.DataAnnotations;

namespace AttendanceTrackerApi.Dtos
{
    /// <summary>
    /// DTO pro registraci nového uživatele (co má přijít z API).
    /// </summary>
    public class RegisterUserDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
