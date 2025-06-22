using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceRecordsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceRecordsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Získat všechny záznamy docházky.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetAttendanceRecords()
        {
            return await _context.AttendanceRecords.ToListAsync();
        }

        /// <summary>
        /// Získat všechny záznamy docházky pro konkrétního uživatele.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<AttendanceRecord>>> GetRecordsByUser(int userId)
        {
            var records = await _context.AttendanceRecords
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return records;
        }

        /// <summary>
        /// Přidat nový záznam docházky.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<AttendanceRecord>> CreateAttendanceRecord(AttendanceRecord record)
        {
            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAttendanceRecords), new { id = record.Id }, record);
        }

        /// <summary>
        /// Aktualizovat existující záznam docházky.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendanceRecord(int id, AttendanceRecord updatedRecord)
        {
            if (id != updatedRecord.Id)
            {
                return BadRequest("ID v URL a těle se neshoduje.");
            }

            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            // Aktualizace hodnot
            record.UserId = updatedRecord.UserId;
            record.Date = updatedRecord.Date;
            record.ArrivalTime = updatedRecord.ArrivalTime;
            record.DepartureTime = updatedRecord.DepartureTime;
            record.Note = updatedRecord.Note;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Smazat záznam docházky podle ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceRecord(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
