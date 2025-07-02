using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Dtos;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Výpis všech uživatelů (BEZ hash hesla, bezpečné!)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            // Promapujeme User → UserDto, hash hesla nikdy nevracíme!
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Detail uživatele podle ID (také přes DTO)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(userDto);
        }

        /// <summary>
        /// Přidat nového uživatele přes DTO s validací. Heslo bude zahashováno.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        /// <summary>
        /// Aktualizace uživatele (pro zjednodušení zatím přes User model, doporučuji později DTO).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Smazat uživatele podle ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
