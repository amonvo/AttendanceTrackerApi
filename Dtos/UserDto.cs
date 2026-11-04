using AttendanceTrackerApi.Models;
namespace AttendanceTrackerApi.Dtos;
public record UserDto(int Id, string Username, string Email, string Role, string Position)
{
    public static UserDto From(User user) => new(user.Id, user.Username, user.Email, user.Role, user.Position);
}
