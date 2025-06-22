using Microsoft.EntityFrameworkCore;
using AttendanceTrackerApi.Models;

namespace AttendanceTrackerApi.Data
{
    /// <summary>
    /// Kontext databáze pro AttendanceTrackerApi.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Tabulka uživatelů.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Tabulka záznamů docházky.
        /// </summary>
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    }
}
