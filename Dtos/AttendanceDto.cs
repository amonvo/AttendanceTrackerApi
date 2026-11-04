using System.ComponentModel.DataAnnotations;
namespace AttendanceTrackerApi.Dtos;
public class AttendanceDto : IValidatableObject
{
    [Range(1, int.MaxValue)] public int UserId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan DepartureTime { get; set; }
    [StringLength(2000)] public string? Note { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Date == default || Date.TimeOfDay != TimeSpan.Zero)
            yield return new ValidationResult("Enter a calendar date without a time.", [nameof(Date)]);
        if (ArrivalTime < TimeSpan.Zero || DepartureTime >= TimeSpan.FromDays(1) || DepartureTime <= ArrivalTime)
            yield return new ValidationResult("Departure must be after arrival on the same day.", [nameof(DepartureTime)]);
    }
}
