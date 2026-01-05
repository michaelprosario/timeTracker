# TimeTracker

A modern time tracking application built with ASP.NET Core, PostgreSQL, and Clean Architecture principles.

## Features

### User Management
- **User Registration** - Create new accounts with email and password
- **User Login** - Secure session-based authentication
- **User Dashboard** - Personalized dashboard with quick access to time tracking features

### Time Tracking
- **Time Sheet Management** - Create and manage weekly time sheets
- **Time Entry Creation** - Log work hours with start/end times
- **Automatic Calculations** - Hours automatically calculated from start and end times
- **Project & Work Type Tracking** - Categorize entries by project and work type
- **Time Sheet Status** - Open/Closed status management
- **Notes Support** - Add detailed notes to time entries

### User Interface
- **Responsive Design** - Modern Bootstrap-based UI
- **Interactive Dashboard** - Easy navigation to all features
- **Real-time Validation** - Form validation with helpful error messages
- **Visual Feedback** - Success/error notifications for all actions

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Language**: C# with .NET 10
- **Database**: PostgreSQL 17
- **ORM**: Entity Framework Core 10.0
- **Architecture**: Clean Architecture (Core, Infrastructure, Web layers)
- **UI Framework**: Bootstrap 5
- **Containerization**: Docker & Docker Compose

## Project Structure

```
timeTracker/
├── src/
│   ├── TimeTracker.Core/           # Domain entities, interfaces, services
│   │   ├── Entities/               # Domain models
│   │   ├── Interfaces/             # Repository interfaces
│   │   ├── Services/               # Business logic
│   │   ├── Commands/               # CQRS commands
│   │   └── Queries/                # CQRS queries
│   ├── TimeTracker.Infrastructure/ # Data access, repositories
│   │   ├── Data/                   # DbContext
│   │   ├── Repositories/           # Repository implementations
│   │   └── Migrations/             # EF Core migrations
│   └── TimeTracker.Web/            # Web application
│       ├── Controllers/            # MVC controllers
│       ├── Views/                  # Razor views
│       ├── Models/                 # View models
│       └── wwwroot/                # Static files
└── tests/
    └── TimeTracker.Core.Tests/     # Unit tests
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 17](https://www.postgresql.org/download/) or Docker
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

### Running with Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/michaelprosario/timeTracker.git
   cd timeTracker
   ```

2. **Start the application**
   ```bash
   docker-compose up --build
   ```

3. **Access the application**
   - Web Application: http://localhost:5000
   - HTTPS: https://localhost:5001

4. **Stop the application**
   ```bash
   docker-compose down
   ```

See [DOCKER.md](DOCKER.md) for more Docker commands and options.

### Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/michaelprosario/timeTracker.git
   cd timeTracker
   ```

2. **Start PostgreSQL**
   
   Using Docker:
   ```bash
   cd pgDockerCompose
   docker network create postgres
   docker-compose up -d
   ```

3. **Update database connection string** (if needed)
   
   Edit `src/TimeTracker.Web/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=timetracker;Username=postgres;Password=Foobar321"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   cd src/TimeTracker.Web
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```
   
   Or use the provided script:
   ```bash
   ./run.sh
   ```

6. **Access the application**
   - Open your browser to: https://localhost:5001 or http://localhost:5000

## Usage Guide

### First Time Setup

1. **Register an Account**
   - Navigate to the Register page
   - Enter your email, password, first name, and last name
   - Password must be at least 8 characters with uppercase, lowercase, number, and special character

2. **Login**
   - Use your registered email and password
   - You'll be redirected to the Dashboard

### Creating Time Entries

1. **Create/Open Time Sheet**
   - From the Dashboard, click "Start Time Sheet" or "View Current Week"
   - This creates or opens your time sheet for the current week

2. **Add Time Entry**
   - Select a Project from the dropdown
   - Select a Work Type from the dropdown
   - Choose the date
   - Enter Start Time (e.g., 09:00)
   - Enter End Time (e.g., 17:00)
   - Hours are automatically calculated
   - Add optional notes
   - Click "Add Entry"

3. **Manage Entries**
   - View all entries in the time sheet details page
   - Delete entries if needed (only when time sheet is open)
   - Total hours are automatically calculated

4. **Close Time Sheet**
   - Click "Close Timesheet" when all entries are complete
   - Closed time sheets cannot be modified

## Database Schema

### Main Tables
- **Users** - User accounts and authentication
- **TimeSheets** - Weekly time sheet records
- **TimeEntries** - Individual time entries
- **Projects** - Project codes and names
- **WorkTypes** - Work type codes and names

## Development

### Running Tests
```bash
cd tests/TimeTracker.Core.Tests
dotnet test
```

### Adding Migrations
```bash
cd src/TimeTracker.Web
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Building the Solution
```bash
dotnet build
```

## API Endpoints

The application uses traditional MVC controllers. Key routes:

- `GET /` - Home page
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Register new user
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Authenticate user
- `GET /Account/Logout` - Logout
- `GET /Home/Dashboard` - User dashboard
- `GET /TimeSheet` - List all time sheets
- `GET /TimeSheet/Details/{id}` - Time sheet details
- `GET /TimeSheet/Create` - Create/get current week time sheet
- `POST /TimeSheet/CreateEntry` - Add time entry
- `POST /TimeSheet/DeleteEntry` - Delete time entry
- `POST /TimeSheet/CloseTimeSheet` - Close time sheet

## Architecture

### Clean Architecture Layers

1. **Core Layer** (`TimeTracker.Core`)
   - Domain entities
   - Business logic
   - Interfaces (no dependencies on infrastructure)

2. **Infrastructure Layer** (`TimeTracker.Infrastructure`)
   - Data access (EF Core)
   - Repository implementations
   - External service integrations

3. **Web Layer** (`TimeTracker.Web`)
   - MVC controllers
   - Razor views
   - View models
   - Dependency injection setup

### Design Patterns
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **CQRS** - Command/Query separation
- **Result Pattern** - Error handling with AppResult

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.

## Contact

- GitHub: [@michaelprosario](https://github.com/michaelprosario)
- Repository: [timeTracker](https://github.com/michaelprosario/timeTracker)