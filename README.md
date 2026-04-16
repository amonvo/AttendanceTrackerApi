# AttendanceTrackerApi

![Build](https://github.com/amonvo/AttendanceTrackerApi/actions/workflows/build.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

REST API for employee attendance tracking built with ASP.NET Core 8, Entity Framework Core, and SQLite. Part of the [AttendanceTracker](https://github.com/amonvo/attendance-tracker-frontend) full-stack project.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 9 |
| Database | SQLite |
| Security | ASP.NET Core Identity PasswordHasher |
| Docs | Swagger / Swashbuckle |
| CI | GitHub Actions |
| Container | Docker |

## Features

- User management with roles (`admin` / `user`)
- Password hashing — plain-text passwords never stored
- Attendance records per user (arrival, departure, date, notes)
- Full CRUD for Users and AttendanceRecords
- DTO pattern — sensitive data never exposed
- EF Core migrations — versioned schema
- CORS pre-configured for frontend on `http://localhost:5173`
- Swagger UI in Development mode

## API Endpoints

### Users — `/api/users`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |

### Attendance — `/api/attendancerecords`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/attendancerecords` | Get all records |
| GET | `/api/attendancerecords/user/{userId}` | Get by user |
| POST | `/api/attendancerecords` | Create record |
| PUT | `/api/attendancerecords/{id}` | Update record |
| DELETE | `/api/attendancerecords/{id}` | Delete record |

## Getting Started

### Option A — Docker (recommended)

```bash
git clone https://github.com/amonvo/AttendanceTrackerApi.git
cd AttendanceTrackerApi
docker build -t attendance-api .
docker run -p 5000:8080 attendance-api
```

### Option B — Local

```bash
git clone https://github.com/amonvo/AttendanceTrackerApi.git
cd AttendanceTrackerApi
dotnet ef database update
dotnet run
```

API runs on `https://localhost:7024` · Swagger at `/swagger`

### Full Stack (Docker Compose)

```bash
git clone https://github.com/amonvo/AttendanceTrackerApi.git
git clone https://github.com/amonvo/attendance-tracker-frontend.git
cd ..
docker-compose up --build
```

- Frontend: `http://localhost:3000`
- Backend API: `http://localhost:5000`

## Project Structure

AttendanceTrackerApi/
├── Controllers/
│   ├── AttendanceRecordsController.cs
│   └── UsersController.cs
├── Data/
│   └── AppDbContext.cs
├── Dtos/
│   ├── RegisterUserDto.cs
│   ├── UpdateUserDto.cs
│   └── UserDto.cs
├── Migrations/
├── Models/
│   ├── AttendanceRecord.cs
│   └── User.cs
├── .github/workflows/
│   └── build.yml
├── Dockerfile
└── Program.cs

## Related

- [attendance-tracker-frontend](https://github.com/amonvo/attendance-tracker-frontend) — React frontend

## License

MIT