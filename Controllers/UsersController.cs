using System.Security.Claims;
using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Dtos;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AttendanceTrackerApi.Controllers;
[ApiController, Authorize, Route("api/users")]
public class UsersController(AppDbContext db, IPasswordHasher<User> hasher) : ControllerBase
{
    private int CurrentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = db.Users.AsNoTracking();
        if (!User.IsInRole("admin")) users = users.Where(u => u.Id == CurrentId);
        return Ok(await users.OrderBy(u => u.Username).Select(u => new UserDto(u.Id, u.Username, u.Email, u.Role, u.Position)).ToListAsync());
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        if (!User.IsInRole("admin") && id != CurrentId) return Forbid();
        var user = await db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(UserDto.From(user));
    }
    [HttpPost, Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(RegisterUserDto dto)
    {
        var user = new User { Username = dto.Username.Trim(), Email = dto.Email.Trim(), Role = dto.Role, Position = dto.Position.Trim() };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new ProblemDetails { Title = "Username or email already exists.", Status = 409 }); }
        return CreatedAtAction(nameof(Get), new { id = user.Id }, UserDto.From(user));
    }
    [HttpPut("{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, UpdateUserDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Id == CurrentId && dto.Role != "admin") return Conflict(new ProblemDetails { Title = "You cannot remove your own administrator role.", Status = 409 });
        user.Username = dto.Username.Trim(); user.Email = dto.Email.Trim(); user.Role = dto.Role; user.Position = dto.Position.Trim();
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new ProblemDetails { Title = "Username or email already exists.", Status = 409 }); }
        return Ok(UserDto.From(user));
    }
    [HttpDelete("{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == CurrentId) return Conflict(new ProblemDetails { Title = "You cannot delete your own account.", Status = 409 });
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (await db.AttendanceRecords.AnyAsync(r => r.UserId == id)) return Conflict(new ProblemDetails { Title = "This user has attendance history. Remove those records explicitly first.", Status = 409 });
        db.Users.Remove(user); await db.SaveChangesAsync(); return NoContent();
    }
}
