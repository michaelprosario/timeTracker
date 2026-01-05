# Time Tracker Application

A bi-weekly time tracking system built using ASP.NET MVC, following Clean Architecture principles.

## Architecture Overview

This application follows **Clean Architecture** as advocated by Steve Smith (Ardalis), with clear separation of concerns:

### Projects

- **TimeTracker.Core** - Domain entities, interfaces, business logic (no dependencies)
- **TimeTracker.Infrastructure** - Data access, EF Core, repository implementations
- **TimeTracker.Web** - ASP.NET MVC controllers and views
- **TimeTracker.Core.Tests** - Unit tests using NUnit and NSubstitute

### Key Patterns Implemented

1. **Clean Architecture** - Dependencies point inward, Core has zero infrastructure dependencies
2. **Repository Pattern** - Abstraction over data access with Unit of Work
3. **CQRS** - Commands and queries clearly separated
4. **Result Pattern** - Consistent error handling via `AppResult<T>`
5. **Dependency Injection** - All dependencies injected via constructor

## Domain Model

### Entities

- **User** - System users with authentication
- **TimeSheet** - 14-day pay period time tracking (Monday to 2 weeks)
- **TimeEntry** - Individual time entries with project and work type
- **Project** - Projects for time allocation
- **WorkType** - Categories of work (DEV, MEET, TEST, etc.)

### Business Rules

- Timesheets cover exactly 14 days starting on Monday
- Users can only access their own timesheets and entries
- Password must be 8+ characters with uppercase, lowercase, number, and special character
- Hours must be between 0.25 and 24 per entry
- Closed timesheets become read-only
- Only active projects and work types can receive new entries

## Technology Stack

- **Framework**: ASP.NET Core MVC 10
- **Database**: PostgreSQL with EF Core
- **Testing**: NUnit with NSubstitute for mocking
- **ORM**: Entity Framework Core 10
- **Containerization**: Docker Compose for PostgreSQL

## Prerequisites

- .NET 10 SDK
- Docker and Docker Compose
- PostgreSQL (via Docker)

## Getting Started

### 1. Start PostgreSQL Database

```bash
cd pgDockerCompose
docker network create postgres
docker-compose up -d
```

This starts:
- PostgreSQL on port 5432
- pgAdmin on port 5050

### 2. Apply Database Migrations

The database should already be created. If not:

```bash
cd src/TimeTracker.Web
dotnet ef database update --project ../TimeTracker.Infrastructure/TimeTracker.Infrastructure.csproj
```

### 3. Run the Application

```bash
cd src/TimeTracker.Web
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

### 4. Run Tests

```bash
dotnet test
```

## Database Connection

Default connection string (configured in `appsettings.json`):

```
Host=localhost;Port=5432;Database=timetracker;Username=postgres;Password=Foobar321
```

## Seed Data

The application seeds the following data on startup:

**Projects:**
- INTERNAL - Internal Tasks
- TRAINING - Training & Development
- PROJECT-A - Project Alpha

**Work Types:**
- DEV - Development
- MEET - Meetings
- TEST - Testing
- ADMIN - Administration
- TRAIN - Training
- SUPPORT - Support

## Usage Flow

1. **Register** - Create a new user account
2. **Login** - Authenticate with email and password
3. **View Timesheets** - See all your timesheet periods
4. **Add Time Entries** - Log hours against projects and work types
5. **Close Timesheet** - Finalize a period

## API Structure

### Core Services

- `UserService` - User registration and authentication
- `TimeSheetService` - Timesheet management
- `TimeEntryService` - Time entry CRUD operations

### Controllers

- `AccountController` - Registration and login
- `TimeSheetController` - Timesheet and entry management
- `HomeController` - Landing page

## Clean Architecture Benefits

1. **Testability** - Core business logic easily tested without infrastructure
2. **Maintainability** - Clear separation of concerns
3. **Flexibility** - Easy to swap out infrastructure (e.g., different database)
4. **Independence** - Core has no framework dependencies

## Project Structure

```
TimeTracker/
├── src/
│   ├── TimeTracker.Core/              # Domain layer
│   │   ├── Entities/                  # Domain entities
│   │   ├── Interfaces/                # Repository interfaces
│   │   ├── Services/                  # Business logic
│   │   ├── Commands/                  # Command objects (CQRS)
│   │   ├── Queries/                   # Query objects (CQRS)
│   │   └── Common/                    # AppResult pattern
│   ├── TimeTracker.Infrastructure/    # Data access layer
│   │   ├── Data/                      # DbContext
│   │   └── Repositories/              # Repository implementations
│   └── TimeTracker.Web/               # Presentation layer
│       ├── Controllers/               # MVC controllers
│       ├── Views/                     # Razor views
│       └── Models/                    # View models
├── tests/
│   └── TimeTracker.Core.Tests/        # Unit tests
├── pgDockerCompose/                   # Docker Compose for PostgreSQL
└── Prompts/                           # Design documents
```

## Testing Strategy

Tests focus on the Core business logic layer:

- **Unit Tests** - Test services in isolation using NSubstitute mocks
- **No Infrastructure Tests** - Infrastructure is implementation detail
- **Repository Mocking** - All data access is mocked via interfaces

Example test:

```csharp
[Test]
public async Task RegisterUserAsync_WithValidData_ReturnsSuccess()
{
    // Arrange
    var command = new RegisterUserCommand { ... };
    _userRepository.EmailExistsAsync(Arg.Any<string>()).Returns(false);
    
    // Act
    var result = await _userService.RegisterUserAsync(command);
    
    // Assert
    Assert.That(result.Success, Is.True);
}
```

## Future Enhancements

- Add authorization middleware
- Implement reporting features (time utilization, work type summary)
- Add API endpoints for mobile/external clients
- Implement real-time timer functionality
- Add export to Excel/PDF
- Team and manager views
- Approval workflows
- Budget tracking

## License

This project is for educational and demonstration purposes.

## References

- [Clean Architecture by Ardalis](https://github.com/ardalis/CleanArchitecture)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
