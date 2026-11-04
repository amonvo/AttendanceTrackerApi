using System.ComponentModel.DataAnnotations;
namespace AttendanceTrackerApi.Dtos;

public class UpdateUserDto : IValidatableObject
{
    [Required, StringLength(80, MinimumLength = 3)] public string Username { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, RegularExpression("^(admin|user)$")] public string Role { get; set; } = "user";
    [Required(AllowEmptyStrings = true), StringLength(80)] public string Position { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Username.Trim().Length < 3)
            yield return new ValidationResult("Username needs at least three non-padding characters.", [nameof(Username)]);
    }
}
