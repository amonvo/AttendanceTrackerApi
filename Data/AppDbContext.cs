using AttendanceTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
namespace AttendanceTrackerApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(u => u.Username).UseCollation("NOCASE");
        modelBuilder.Entity<User>().Property(u => u.Email).UseCollation("NOCASE");
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<AttendanceRecord>().HasIndex(r => new { r.UserId, r.Date });
        modelBuilder.Entity<AttendanceRecord>().HasOne<User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
