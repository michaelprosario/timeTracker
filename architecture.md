# TimeTracker - Architecture Specification

## Document Information
- **Project**: TimeTracker
- **Version**: 1.0
- **Last Updated**: January 12, 2026
- **Target Framework**: .NET 10.0
- **Database**: PostgreSQL 17

## Executive Summary

TimeTracker is a web-based time tracking application built using Clean Architecture principles. The application enables users to manage time entries, organize work by projects and work types, and generate time reports. The system is designed with separation of concerns, maintainability, and testability as core principles.

## Architecture Overview

### Architectural Style
The application follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture), organized into three primary layers:

1. **Core Layer** (Domain & Business Logic)
2. **Infrastructure Layer** (Data Access & External Dependencies)
3. **Presentation Layer** (Web UI)

### Design Patterns
- **Repository Pattern** - Abstracts data access logic
- **Unit of Work Pattern** - Manages transactional consistency
- **CQRS (Command Query Responsibility Segregation)** - Separates read and write operations
- **Dependency Injection** - Manages component dependencies
- **MVC (Model-View-Controller)** - Web presentation pattern

---

## System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│                   (TimeTracker.Web)                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Controllers  │  │    Views     │  │    Models    │      │
│  │   (MVC)      │  │   (Razor)    │  │  (ViewModels)│      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────┬────────────────────────────────────┘
                         │ Depends on
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      Core Layer                              │
│                 (TimeTracker.Core)                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ Entities │  │ Services │  │ Commands │  │  Queries │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           Interfaces (Repository, UoW)               │   │
│  └──────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │ Implemented by
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│              (TimeTracker.Infrastructure)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Repositories │  │   DbContext  │  │  Migrations  │      │
│  │    (EF)      │  │   (EF Core)  │  │              │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
                   ┌──────────┐
                   │PostgreSQL│
                   │    DB    │
                   └──────────┘
```

---

## Layer Details

### 1. Core Layer (TimeTracker.Core)

**Purpose**: Contains the domain model, business logic, and application use cases. This layer is independent of any external frameworks or infrastructure concerns.

**Dependencies**: None (no external dependencies)

#### 1.1 Entities (Domain Models)

Domain entities represent the core business objects:

- **User**
  - Properties: Id, Email, PasswordHash, FirstName, LastName, CreatedAt, IsActive
  - Relationships: Has many TimeSheets

- **TimeSheet**
  - Properties: Id, UserId, StartDate, EndDate, Status, TotalHours, CreatedAt, LastModifiedAt
  - Relationships: Belongs to User, Has many TimeEntries
  - Business Rules: Represents a 2-week period starting on Monday

- **TimeEntry**
  - Properties: Id, TimeSheetId, ProjectCode, WorkTypeCode, StartTime, EndTime, Hours, EntryDate, Notes, CreatedAt, CreatedBy, LastModifiedAt
  - Relationships: Belongs to TimeSheet, Project, and WorkType
  - Business Rules: Hours calculated from StartTime and EndTime

- **Project**
  - Properties: Code (PK), Name, Description, IsActive
  - Relationships: Has many TimeEntries

- **WorkType**
  - Properties: Code (PK), Name, Description, IsActive
  - Relationships: Has many TimeEntries

#### 1.2 Interfaces

Repository interfaces define contracts for data access:

- `IUserRepository` - User CRUD operations
- `ITimeSheetRepository` - TimeSheet operations with period-based queries
- `ITimeEntryRepository` - TimeEntry operations with filtering
- `IProjectRepository` - Project management
- `IWorkTypeRepository` - WorkType management
- `IUnitOfWork` - Transaction management and repository access

#### 1.3 Services (Business Logic)

Domain services encapsulate business logic:

- **UserService** - User registration, authentication, profile management
- **TimeSheetService** - TimeSheet creation, closure, period management
- **TimeEntryService** - Time entry CRUD, hour calculations, validation
- **ProjectService** - Project management
- **WorkTypeService** - Work type management

#### 1.4 Commands & Queries (CQRS)

**Commands** (Write Operations):
- `CreateUserCommand`
- `CreateOrGetTimeSheetCommand`
- `CreateTimeEntryCommand`
- `UpdateTimeEntryCommand`
- `DeleteTimeEntryCommand`
- `CloseTimeSheetCommand`

**Queries** (Read Operations):
- `GetTimeSheetByIdQuery`
- `GetTimeEntriesForTimeSheetQuery`
- `GetOpenTimeSheetsQuery`
- `ProjectTimeReportQuery`

#### 1.5 Common

- **AppResult<T>** - Standardized result object for operation outcomes with success/failure states and messages

---

### 2. Infrastructure Layer (TimeTracker.Infrastructure)

**Purpose**: Implements data access and external dependencies. Handles all infrastructure concerns including database access, external services, and cross-cutting concerns.

**Dependencies**: TimeTracker.Core, Entity Framework Core, Npgsql (PostgreSQL provider)

#### 2.1 Data Access

- **TimeTrackerDbContext** - EF Core DbContext
  - DbSets: Users, TimeSheets, TimeEntries, Projects, WorkTypes
  - Configuration: Fluent API for entity relationships, constraints, and indexes
  - Seed Data: Initial Projects and WorkTypes

#### 2.2 Repositories

Concrete implementations of repository interfaces:

- **UserRepository** - User data access with email uniqueness validation
- **TimeSheetRepository** - TimeSheet queries with period-based filtering
- **TimeEntryRepository** - TimeEntry CRUD with project/worktype filtering
- **ProjectRepository** - Project management
- **WorkTypeRepository** - WorkType management
- **UnitOfWork** - Transaction coordination and SaveChanges implementation

#### 2.3 Migrations

- EF Core migrations for database schema management
- Migration: `20260105051257_InitialCreate`
- Includes SQL script for manual deployment if needed

#### 2.4 Database Design

**Key Design Decisions**:
- GUIDs for primary keys (Users, TimeSheets, TimeEntries)
- String codes for Projects and WorkTypes (business keys)
- Cascade delete for owned entities (TimeSheet → TimeEntries)
- Restrict delete for reference data (Project, WorkType)
- Indexes on foreign keys and frequently queried fields
- Decimal(18,2) for monetary/hour values
- Unique constraint on User.Email

---

### 3. Presentation Layer (TimeTracker.Web)

**Purpose**: Web user interface built with ASP.NET Core MVC. Handles HTTP requests, user interaction, and view rendering.

**Dependencies**: TimeTracker.Core, TimeTracker.Infrastructure

#### 3.1 Controllers

MVC Controllers handle HTTP requests and orchestrate service calls:

- **AccountController** - User registration, login, logout, profile
- **HomeController** - Dashboard and landing pages
- **TimeSheetController** - TimeSheet management
- **ProjectController** - Project CRUD
- **WorkTypeController** - WorkType CRUD
- **ReportsController** - Time reports and analytics
- **BaseController** - Common functionality (session management, user context)

#### 3.2 Views

Razor views for UI rendering:
- Layout templates with Bootstrap 5
- Responsive design
- Partial views for reusable components
- Client-side validation

#### 3.3 Models

View Models for data transfer between controllers and views:
- Form input models with validation attributes
- Display models for view rendering
- Error models for exception handling

#### 3.4 Static Assets

- CSS stylesheets
- JavaScript files
- Bootstrap 5 framework
- jQuery for client-side interactions

#### 3.5 Configuration

- **appsettings.json** - Application configuration
- **appsettings.Development.json** - Development-specific settings
- **Program.cs** - Application startup and DI configuration

---

## Cross-Cutting Concerns

### Dependency Injection

All dependencies are registered in Program.cs:

```csharp
// Database
AddDbContext<TimeTrackerDbContext>

// Repositories
AddScoped<IUserRepository, UserRepository>
AddScoped<ITimeSheetRepository, TimeSheetRepository>
AddScoped<ITimeEntryRepository, TimeEntryRepository>
AddScoped<IProjectRepository, ProjectRepository>
AddScoped<IWorkTypeRepository, WorkTypeRepository>
AddScoped<IUnitOfWork, UnitOfWork>

// Services
AddScoped<UserService>
AddScoped<TimeSheetService>
AddScoped<TimeEntryService>
AddScoped<ProjectService>
AddScoped<WorkTypeService>
```

### Session Management

- **Implementation**: In-memory distributed cache
- **Duration**: 2 hours idle timeout
- **Usage**: Stores UserId and UserName for authentication
- **Security**: HttpOnly and Essential cookies enabled

### Authentication & Authorization

- **Type**: Session-based authentication
- **Storage**: Session cookies
- **Login**: Email and password validation against hashed passwords
- **Authorization**: BaseController checks session for authenticated user

### Error Handling

- Development: Developer exception page
- Production: Custom error handling with `/Home/Error` route
- AppResult<T> pattern for business operation results

### Logging

- Built-in ASP.NET Core logging
- Console logger for development
- Configurable log levels via appsettings.json

---

## Data Flow

### Typical Request Flow

1. **HTTP Request** → Controller Action
2. **Controller** → Validates input, extracts session data
3. **Controller** → Calls Domain Service
4. **Service** → Executes business logic using Commands/Queries
5. **Service** → Calls Repository through UnitOfWork
6. **Repository** → Executes EF Core query against DbContext
7. **DbContext** → Translates to SQL and executes against PostgreSQL
8. **Result** flows back through layers
9. **Controller** → Returns View or RedirectToAction
10. **View** → Rendered as HTML response

### Example: Creating a Time Entry

```
User submits form
  ↓
TimeSheetController.CreateEntry(model)
  ↓
Validates model & session
  ↓
TimeEntryService.CreateTimeEntryAsync(command)
  ↓
Business validation (project/worktype exists, timesheet open)
  ↓
Creates TimeEntry entity
  ↓
_unitOfWork.TimeEntries.AddAsync(entry)
  ↓
_unitOfWork.SaveChangesAsync()
  ↓
DbContext.SaveChanges() → SQL INSERT
  ↓
Returns AppResult<TimeEntry>
  ↓
Controller redirects to TimeSheet details
```

---

## Database Schema

### Entity Relationship Diagram

```
┌──────────────┐
│    Users     │
│──────────────│
│ Id (PK)      │───┐
│ Email (UQ)   │   │
│ PasswordHash │   │
│ FirstName    │   │
│ LastName     │   │
│ CreatedAt    │   │
│ IsActive     │   │
└──────────────┘   │
                   │ 1:N
                   │
       ┌───────────┘
       │
       ▼
┌──────────────┐
│  TimeSheets  │
│──────────────│
│ Id (PK)      │───┐
│ UserId (FK)  │   │
│ StartDate    │   │
│ EndDate      │   │
│ Status       │   │
│ TotalHours   │   │
│ CreatedAt    │   │
└──────────────┘   │ 1:N
                   │
       ┌───────────┘
       │
       ▼
┌──────────────┐       ┌──────────────┐
│ TimeEntries  │       │   Projects   │
│──────────────│       │──────────────│
│ Id (PK)      │       │ Code (PK)    │
│ TimeSheetId  │◄──┐   │ Name         │
│ ProjectCode  │───┼──►│ Description  │
│ WorkTypeCode │   │   │ IsActive     │
│ StartTime    │   │   └──────────────┘
│ EndTime      │   │
│ Hours        │   │   ┌──────────────┐
│ EntryDate    │   │   │  WorkTypes   │
│ Notes        │   │   │──────────────│
│ CreatedAt    │   └──►│ Code (PK)    │
│ CreatedBy    │       │ Name         │
└──────────────┘       │ Description  │
                       │ IsActive     │
                       └──────────────┘
```

### Key Relationships

- **User** 1:N **TimeSheet** (Cascade Delete)
- **TimeSheet** 1:N **TimeEntry** (Cascade Delete)
- **Project** 1:N **TimeEntry** (Restrict Delete)
- **WorkType** 1:N **TimeEntry** (Restrict Delete)

---

## Deployment Architecture

### Containerization

**Docker Compose Setup**:
- **web** container: ASP.NET Core application
- **postgres** network: External network for PostgreSQL communication

### Environment Configuration

- **Development**: Local PostgreSQL or Docker container
- **Connection String**: Injected via environment variables
- **Port Mapping**: Container 8080 → Host 5000

### Startup Process

1. Docker Compose starts web container
2. Application connects to PostgreSQL on external network
3. EF Core applies migrations (if configured)
4. Application listens on port 8080
5. Accessible via http://localhost:5000

---

## Technology Stack Summary

### Core Technologies
- **.NET 10.0** - Application framework
- **C# 10+** - Programming language with nullable reference types enabled
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL 17** - Relational database
- **Npgsql** - PostgreSQL provider for EF Core

### UI Technologies
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library

### Development Tools
- **Docker & Docker Compose** - Containerization
- **Git** - Version control
- **.NET CLI** - Build and development tools

---

## Design Principles

### SOLID Principles

1. **Single Responsibility Principle**
   - Each service handles one domain aggregate
   - Controllers handle only HTTP concerns
   - Repositories handle only data access

2. **Open/Closed Principle**
   - Services depend on interfaces, not implementations
   - New repositories can be added without changing services

3. **Liskov Substitution Principle**
   - Repository implementations can be swapped
   - Services work with any IRepository implementation

4. **Interface Segregation Principle**
   - Specific repository interfaces for each entity
   - No "god" interface with all methods

5. **Dependency Inversion Principle**
   - Core depends on abstractions (interfaces)
   - Infrastructure implements abstractions
   - Web depends on Core abstractions

### Clean Architecture Benefits

1. **Independence of Frameworks** - Business logic doesn't depend on EF Core or ASP.NET
2. **Testability** - Business logic can be tested without database or web server
3. **Independence of UI** - Could add Web API without changing business logic
4. **Independence of Database** - Could switch to SQL Server with minimal changes
5. **Independence of External Dependencies** - External services can be mocked

---

## Testing Strategy

### Test Project Structure

**TimeTracker.Core.Tests** - Unit tests for:
- Service business logic
- Domain entity behavior
- Command/Query validation
- AppResult patterns

### Testing Approach

- **Unit Tests**: Services with mocked repositories
- **Integration Tests**: Repository tests with in-memory database (future)
- **UI Tests**: Controller tests with mocked services (future)

---

## Security Considerations

### Authentication
- Password hashing (implementation uses simple hashing, should use BCrypt/Argon2 in production)
- Session-based authentication with secure cookies

### Data Protection
- SQL injection protection via EF Core parameterized queries
- Input validation on models
- XSS protection via Razor encoding

### Areas for Enhancement
- Implement proper password hashing (BCrypt, Argon2)
- Add HTTPS enforcement
- Implement CSRF protection
- Add authorization policies
- Implement audit logging
- Add rate limiting

---

## Performance Considerations

### Database Optimization
- Indexes on foreign keys and frequently queried fields
- Eager loading for related entities where appropriate
- Pagination for large result sets (future enhancement)

### Caching Strategy
- Session data cached in memory
- Static reference data (Projects, WorkTypes) candidates for caching

### Areas for Enhancement
- Implement response caching
- Add query result caching
- Optimize N+1 query issues
- Add connection pooling configuration

---

## Scalability Considerations

### Current Limitations
- In-memory session storage (single server)
- No distributed caching
- Direct database access per request

### Future Enhancements
- Redis for distributed session storage
- Read replicas for reporting queries
- CQRS with separate read/write databases
- Message queue for async operations
- API Gateway for microservices evolution

---

## Maintenance & Extensibility

### Adding New Features

**To add a new entity**:
1. Create entity in Core/Entities
2. Add DbSet to TimeTrackerDbContext
3. Create repository interface in Core/Interfaces
4. Implement repository in Infrastructure/Repositories
5. Add migration
6. Create service in Core/Services
7. Register in Program.cs DI container
8. Create controller and views in Web layer

### Code Organization
- Feature-based organization within layers
- Consistent naming conventions
- Clear separation of concerns
- Minimal dependencies between features

---

## Monitoring & Observability

### Current Implementation
- Console logging in development
- Exception handling middleware
- AppResult pattern for operation tracking

### Recommended Additions
- Structured logging (Serilog)
- Application Performance Monitoring (APM)
- Health check endpoints
- Metrics collection
- Distributed tracing

---

## Compliance & Standards

### Code Standards
- C# coding conventions
- Nullable reference types enabled
- Async/await for I/O operations
- Using statements for resource management

### Documentation
- XML comments for public APIs
- README files for setup instructions
- This architecture specification

---

## Known Technical Debt

1. **Password Hashing** - Using basic hashing, should use BCrypt or Argon2
2. **Error Messages** - Should be externalized and localized
3. **Validation** - Should be more comprehensive with FluentValidation
4. **Pagination** - Not implemented for list views
5. **API Layer** - No REST API for programmatic access
6. **Real-time Updates** - No SignalR for live updates
7. **File Storage** - No support for attachments or file uploads
8. **Email** - No email notifications
9. **Reporting** - Limited reporting capabilities
10. **Mobile** - No native mobile app or responsive optimization

---

## Future Roadmap

### Phase 1 - Security & Stability
- Implement proper password hashing
- Add comprehensive input validation
- Implement CSRF protection
- Add audit logging

### Phase 2 - Performance & Scale
- Add Redis for distributed caching
- Implement response caching
- Optimize database queries
- Add pagination

### Phase 3 - Features
- REST API layer
- Advanced reporting with charts
- Email notifications
- File attachments
- Time entry approval workflow

### Phase 4 - Architecture Evolution
- Event-driven architecture
- Microservices decomposition
- Real-time updates with SignalR
- GraphQL API

---

## References

### Architecture Patterns
- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- CQRS Pattern by Martin Fowler

### Technologies
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## Conclusion

TimeTracker implements a clean, maintainable architecture that separates concerns and provides a solid foundation for future growth. The use of Clean Architecture principles ensures the codebase remains testable, flexible, and resilient to change. While there are areas for improvement (noted in the technical debt section), the current implementation provides a robust starting point for a production time tracking system.

The modular design allows for incremental improvements and feature additions without requiring major refactoring. The clear separation between layers ensures that changes in one area (e.g., switching databases) have minimal impact on other parts of the system.
