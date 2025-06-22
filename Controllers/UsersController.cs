using AttendanceTrackerApi.Data;
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
        // Bezpečná třída pro hashování a ověření hesla
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Získat všechny uživatele.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        /// <summary>
        /// Získat konkrétního uživatele podle ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        /// <summary>
        /// Přidat nového uživatele. Heslo bude zahashováno před uložením.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            // Zahashujeme heslo, které přišlo v poli PasswordHash
            var hashedPassword = _passwordHasher.HashPassword(user, user.PasswordHash);
            user.PasswordHash = hashedPassword;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Aktualizovat uživatele podle ID. Pokud se mění heslo, zahashuje nové.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest("ID v URL a v těle se neshoduje.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Username = updatedUser.Username;

            // Pokud se heslo změnilo, zahashuj nové
            if (user.PasswordHash != updatedUser.PasswordHash)
                user.PasswordHash = _passwordHasher.HashPassword(user, updatedUser.PasswordHash);

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
