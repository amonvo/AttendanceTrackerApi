using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Dtos;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
namespace AttendanceTrackerApi.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, IPasswordHasher<User> hasher) : ControllerBase
{
    public record LoginDto([Required, StringLength(80)] string Username, [Required, StringLength(128)] string Password);
    [HttpGet("csrf"), AllowAnonymous]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery) => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username.Trim());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new ProblemDetails { Title = "Invalid username or password.", Status = 401 });
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role), new Claim("credential", user.PasswordHash) };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return Ok(UserDto.From(user));
    }
    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me() => Ok(UserDto.From((await db.Users.FindAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)))!));
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return NoContent(); }
}
