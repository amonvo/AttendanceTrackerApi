# Attendance Workspace · API

ASP.NET Core 10 and SQLite backend for a bilingual team attendance workspace. Administrators manage members and records. Members can access and edit their own attendance. The React interface lives in [attendance-tracker-frontend](https://github.com/amonvo/attendance-tracker-frontend).

## Local development

Install .NET 10 SDK. Clone both repositories into sibling directories and check out the matching `codex/professional-attendance` branches while the paired PRs are under review.

```powershell
dotnet restore --locked-mode
$env:Bootstrap__Username = 'admin'
$env:Bootstrap__Email = 'admin@example.com'
$credential = Read-Host 'Choose the initial administrator password (12+ characters)' -AsSecureString
$env:Bootstrap__Password = [System.Net.NetworkCredential]::new('', $credential).Password
dotnet run -- --initialize
Remove-Item Env:Bootstrap__Password
dotnet run --launch-profile http
```

The API listens on `http://localhost:5135`. Start the sibling frontend with `npm ci` and `npm run dev`, then open `http://127.0.0.1:5173`. The Vite proxy keeps requests and authentication on the same origin. Use a consistent hostname throughout your session.

Initialization runs versioned migrations and creates the first administrator only when no administrator exists. Credentials belong in local configuration. Existing identities that conflict with the bootstrap account require deliberate review.

## Docker Compose

Copy `.env.example` to `.env`, set a unique bootstrap password and run `docker compose up --build`. Open `http://localhost:3000`. Only the frontend is published, bound to loopback. SQLite is stored in the `attendance-data` volume. Compose uses development cookie settings for local HTTP. The initializer runs as root solely to prepare volume ownership, while the API runs as the container application user.

## API and security

`GET /api/auth/csrf` returns a request token. Include it as `X-CSRF-TOKEN` on every mutation, including login. Refresh it after identity changes. `POST /api/auth/login` accepts username and password, `GET /api/auth/me` returns the current identity and `POST /api/auth/logout` ends the session. Cookies are HttpOnly and SameSite Strict. Production cookies require HTTPS. Login attempts are rate limited.

`/api/users` supports administrator CRUD and self-only reads for members. Usernames and email addresses are unique ignoring case. Positions are persisted. Self-deletion and self-demotion are blocked. Users with attendance history cannot be deleted until that history is explicitly handled.

`/api/attendancerecords` supports CRUD with ownership checks. List queries accept `from` and `to`, defaulting to the last 90 days, with a maximum 366-day difference. Times represent local wall-clock times on one calendar day. Split overnight work across dates. Departure must follow arrival. Password hashes never appear in DTOs.

## Existing databases

Back up SQLite before upgrading. Test the migration on a copy first. Duplicate usernames/emails ignoring case or orphan attendance records must be resolved before applying the new uniqueness and foreign-key constraints. Initialization fails rather than removing conflicting data. Coordinate deployment of the API and frontend because authentication and validation are new requirements.

## Verification

```sh
dotnet restore --locked-mode
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
```

Integration tests exercise anonymous access, CSRF, ownership, administrator CRUD, validation, history protection, logout and upgrades from the original schema. CI also builds the container.

For production, configure HTTPS, protected persistent data-protection keys, backups and access controls. Account invitation, password recovery, audit trails and payroll rules remain roadmap items. See SECURITY.md. The original project described MIT licensing but did not include a license file. License clarification is pending author review.
