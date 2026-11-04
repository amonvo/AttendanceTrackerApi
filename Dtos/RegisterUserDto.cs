using System.ComponentModel.DataAnnotations;
namespace AttendanceTrackerApi.Dtos;
public class RegisterUserDto : UpdateUserDto
{
    [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
}
