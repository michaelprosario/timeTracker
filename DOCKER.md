# TimeTracker Application

## Running with Docker Compose

### Quick Start

1. **Build and start all services:**
   ```bash
   docker-compose up --build
   ```

2. **Access the application:**
   - Web Application: http://localhost:5000
   - HTTPS: https://localhost:5001
   - PostgreSQL: localhost:5432

3. **Stop the services:**
   ```bash
   docker-compose down
   ```

4. **Stop and remove volumes (clean database):**
   ```bash
   docker-compose down -v
   ```

### Services

- **web**: The .NET 10 TimeTracker web application
  - Ports: 5000 (HTTP), 5001 (HTTPS)
  - Automatically connects to PostgreSQL database
  - Runs migrations on startup

- **postgres**: PostgreSQL 17 database
  - Port: 5432
  - Database: timetracker
  - User: postgres
  - Password: Foobar321

### Development

To run in detached mode:
```bash
docker-compose up -d
```

To view logs:
```bash
docker-compose logs -f web
```

To rebuild after code changes:
```bash
docker-compose up --build web
```

### Database Migrations

Migrations will run automatically when the application starts. If you need to run them manually:

```bash
docker-compose exec web dotnet ef database update --project /app
```
