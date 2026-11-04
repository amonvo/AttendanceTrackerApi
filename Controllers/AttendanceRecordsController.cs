using System.Security.Claims;
using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Dtos;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AttendanceTrackerApi.Controllers;

[ApiController, Authorize, Route("api/attendancerecords")]
public class AttendanceRecordsController(AppDbContext db) : ControllerBase
{
    private int CurrentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool CanAccess(int id) => User.IsInRole("admin") || id == CurrentId;
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var start = from?.Date ?? DateTime.Today.AddDays(-89);
        var end = to?.Date ?? DateTime.Today;
        if (end < start || (end - start).TotalDays > 366) return BadRequest(new ProblemDetails { Title = "Choose a date range of at most 366 days.", Status = 400 });
        var records = db.AttendanceRecords.AsNoTracking().Where(r => r.Date >= start && r.Date <= end);
        if (!User.IsInRole("admin")) records = records.Where(r => r.UserId == CurrentId);
        return Ok(await records.OrderByDescending(r => r.Date).ThenByDescending(r => r.Id).ToListAsync());
    }
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> ByUser(int userId)
    {
        if (!CanAccess(userId)) return Forbid();
        return Ok(await db.AttendanceRecords.AsNoTracking().Where(r => r.UserId == userId && r.Date >= DateTime.Today.AddDays(-89))
            .OrderByDescending(r => r.Date).ToListAsync());
    }
    [HttpPost]
    public async Task<IActionResult> Create(AttendanceDto dto)
    {
        if (!CanAccess(dto.UserId)) return Forbid();
        if (!await db.Users.AnyAsync(u => u.Id == dto.UserId)) return BadRequest(new ProblemDetails { Title = "User does not exist.", Status = 400 });
        var record = new AttendanceRecord { UserId = dto.UserId, Date = dto.Date.Date, ArrivalTime = dto.ArrivalTime, DepartureTime = dto.DepartureTime, Note = dto.Note?.Trim() };
        db.AttendanceRecords.Add(record); await db.SaveChangesAsync(); return StatusCode(201, record);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AttendanceDto dto)
    {
        var record = await db.AttendanceRecords.FindAsync(id);
        if (record is null) return NotFound();
        if (!CanAccess(record.UserId) || !CanAccess(dto.UserId)) return Forbid();
        if (!await db.Users.AnyAsync(u => u.Id == dto.UserId)) return BadRequest(new ProblemDetails { Title = "User does not exist.", Status = 400 });
        record.UserId = dto.UserId; record.Date = dto.Date.Date; record.ArrivalTime = dto.ArrivalTime; record.DepartureTime = dto.DepartureTime; record.Note = dto.Note?.Trim();
        await db.SaveChangesAsync(); return Ok(record);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await db.AttendanceRecords.FindAsync(id);
        if (record is null) return NotFound();
        if (!CanAccess(record.UserId)) return Forbid();
        db.AttendanceRecords.Remove(record); await db.SaveChangesAsync(); return NoContent();
    }
}
