// Dtos/UpdateUserDto.cs
using System.ComponentModel.DataAnnotations;

namespace AttendanceTrackerApi.Dtos
{
    /// <summary>
    /// DTO pro editaci uživatele (z API)
    /// </summary>
    public class UpdateUserDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
