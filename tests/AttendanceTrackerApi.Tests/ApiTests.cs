using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AttendanceTrackerApi.Data;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AttendanceTrackerApi.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string database = Path.Combine(Path.GetTempPath(), $"attendance-test-{Guid.NewGuid()}.db");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Data Source={database};Pooling=False"
        }));
    }
    public async Task Seed()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        foreach (var (name, role) in new[] { ("admin", "admin"), ("member", "user"), ("other", "user") })
        {
            var user = new User { Username = name, Email = $"{name}@example.com", Role = role };
            user.PasswordHash = hasher.HashPassword(user, "Fixture-only-pass-42"); db.Users.Add(user);
        }
        await db.SaveChangesAsync();
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) File.Delete(database); }
}

public class ApiTests : IAsyncLifetime
{
    private readonly ApiFactory factory = new();
    private HttpClient client = null!;
    public async Task InitializeAsync() { client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }); await factory.Seed(); }
    public Task DisposeAsync() { client.Dispose(); factory.Dispose(); return Task.CompletedTask; }
    private async Task Csrf()
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/auth/csrf");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("token").GetString());
    }
    private async Task Login(string name = "admin")
    {
        await Csrf(); var result = await client.PostAsJsonAsync("/api/auth/login", new { username = name, password = "Fixture-only-pass-42" });
        Assert.Equal(HttpStatusCode.OK, result.StatusCode); await Csrf();
    }
    private static object Record(int userId, string arrival = "09:00:00", string departure = "17:00:00") => new { userId, date = DateTime.Today.ToString("yyyy-MM-dd"), arrivalTime = arrival, departureTime = departure, note = "Test" };

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/attendancerecords")]
    [InlineData("/api/auth/me")]
    public async Task Anonymous_cannot_read_data(string path) => Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);

    [Fact]
    public async Task Login_requires_csrf_and_valid_password()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "Fixture-only-pass-42" })).StatusCode);
        await Csrf();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "wrong" })).StatusCode);
    }

    [Fact]
    public async Task Member_sees_only_self_and_cannot_manage_users_or_others_records()
    {
        await Login("member");
        var users = await client.GetFromJsonAsync<JsonElement>("/api/users");
        Assert.Equal(1, users.GetArrayLength()); Assert.Equal("member", users[0].GetProperty("username").GetString());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/users/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/users", new { username = "new", email = "new@example.com", password = "Fixture-only-pass-42", role = "admin" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/attendancerecords", Record(1))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/attendancerecords/user/1")).StatusCode);
    }

    [Fact]
    public async Task Member_can_manage_own_attendance_but_cannot_reassign_it()
    {
        await Login("member");
        var created = await client.PostAsJsonAsync("/api/attendancerecords", Record(2)); Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/attendancerecords/{id}", Record(3))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/attendancerecords/{id}", Record(2, "08:00:00", "16:00:00"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/attendancerecords/{id}")).StatusCode);
    }

    [Fact]
    public async Task Member_cannot_read_update_or_delete_another_members_record()
    {
        await Login(); var created = await client.PostAsJsonAsync("/api/attendancerecords", Record(3));
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        await client.PostAsync("/api/auth/logout", null); await Login("member");
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/attendancerecords")).GetArrayLength());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PutAsJsonAsync($"/api/attendancerecords/{id}", Record(2))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.DeleteAsync($"/api/attendancerecords/{id}")).StatusCode);
    }

    [Fact]
    public async Task Admin_crud_preserves_position_and_never_returns_password_hash()
    {
        await Login();
        var data = new { username = "new-person", email = "new@example.com", password = "Fixture-only-pass-42", role = "user", position = "Designer" };
        var response = await client.PostAsJsonAsync("/api/users", data); Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var raw = await response.Content.ReadAsStringAsync(); Assert.DoesNotContain("password", raw, StringComparison.OrdinalIgnoreCase);
        var user = JsonDocument.Parse(raw).RootElement; var id = user.GetProperty("id").GetInt32(); Assert.Equal("Designer", user.GetProperty("position").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/users", new { username = "NEW-PERSON", email = "second@example.com", password = "Fixture-only-pass-42", role = "user" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/users/{id}", new { username = "new-person", email = "new@example.com", role = "user", position = "Lead" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/users/{id}")).StatusCode);
    }

    [Fact]
    public async Task Protects_history_self_account_and_csrf()
    {
        await Login(); await client.PostAsJsonAsync("/api/attendancerecords", Record(2));
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync("/api/users/2")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync("/api/users/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync("/api/users/1", new { username = "admin", email = "admin@example.com", role = "user" })).StatusCode);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.DeleteAsync("/api/users/3")).StatusCode);
    }

    [Theory]
    [InlineData("18:00:00", "09:00:00")]
    [InlineData("09:00:00", "09:00:00")]
    [InlineData("-01:00:00", "09:00:00")]
    public async Task Rejects_invalid_time_ranges(string start, string end)
    {
        await Login(); Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/attendancerecords", Record(1, start, end))).StatusCode);
    }

    [Fact]
    public async Task Logout_invalidates_session_and_rejects_overlong_queries()
    {
        await Login();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/attendancerecords?from=2020-01-01&to=2026-01-01")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users")).StatusCode);
    }

    [Fact]
    public async Task Upgrade_preserves_legacy_users_and_records()
    {
        using var scope = factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync(); var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("20250620165747_InitialCreate");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO Users (Id, Username, Email, PasswordHash, Role) VALUES (10, 'legacy', 'legacy@example.com', 'fixture', 'user')");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO AttendanceRecords (UserId, Date, ArrivalTime, DepartureTime, Note) VALUES (10, '2026-01-05 00:00:00', '09:00:00', '17:00:00', 'Legacy record')");
        await migrator.MigrateAsync();
        Assert.Equal("legacy", (await db.Users.SingleAsync()).Username);
        Assert.Equal("", (await db.Users.SingleAsync()).Position);
        Assert.Equal("Legacy record", (await db.AttendanceRecords.SingleAsync()).Note);
    }
}
