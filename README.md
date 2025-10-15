# Ore Platform (Stratus Update)

The Stratus update evolves Ore into a production-ready, AI-assisted social media companion. This repository hosts the .NET 9 backend (Clean Architecture) and a React dashboard that together orchestrate content generation, scheduling, approvals, and distribution across platforms.

## Project Structure

| Layer | Highlights |
|-------|------------|
| `src/Domain` | Core entities, value objects, and domain events for users, teams, surveys, content, notifications, and auditing. |
| `src/Application` | CQRS handlers with MediatR, validation pipeline, service abstractions for identity, LLM, storage, cache, scheduling, and auditing. |
| `src/Infrastructure` | EF Core PostgreSQL persistence, ASP.NET Identity, Redis cache, Quartz scheduling, MinIO media storage, OpenAI integration, and migration tooling. |
| `src/Api` | ASP.NET Core Web API host (Swagger enabled). |
| `src/Worker` | Background service host for scheduled jobs and async workflows. |
| `dashboard` | Vite + React + TypeScript administrative dashboard. |
| `scripts` | Cross-environment EF Core migration helpers and workflow automation. |

## Stratus Update Progress

- [x] Solution scaffolding (.NET backend, worker, React dashboard)
- [x] Domain model with entities, value objects, and events
- [x] Application layer handlers, validators, and DI configuration
- [x] Infrastructure services (Identity, EF Core, Redis, Quartz, MinIO, OpenAI)
- [x] Design-time DbContext factory and initial migration (`InitialCreate`)
- [x] Environment-aware migration scripts + GitHub Actions workflow
- [ ] API endpoints (REST + auth wiring)
- [ ] Background worker orchestration & Quartz jobs
- [ ] React dashboard feature build-out
- [ ] Docker compose & deployment automation
- [ ] Comprehensive tests & CI quality gates

## Getting Started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- PostgreSQL, Redis, MinIO (local or containerized)

### Backend

```powershell
# Restore tools & dependencies
 dotnet restore
 dotnet tool restore

# Apply database migrations (Development defaults)
 powershell -ExecutionPolicy Bypass -File .\scripts\migrate.ps1 -Action update

# Run the API
 dotnet run --project src/Api/Ore.Api.csproj

# Run the worker
 dotnet run --project src/Worker/Ore.Worker.csproj
```

Set the following environment variables or update `appsettings.Development.json` before running:

- `ConnectionStrings:Database`
- `ConnectionStrings:Redis`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key` (Base64-encoded secret)
- `OpenAi:ApiKey`
- `Minio:Endpoint`, `Minio:AccessKey`, `Minio:SecretKey`

### Frontend

```powershell
cd dashboard
npm install
npm run dev
```

### Docker Compose

Run the full stack (API, worker, dashboard, Postgres, Redis, MinIO) with Docker once you have Docker Desktop (or an equivalent runtime) installed.

```powershell
docker compose up --build
```

- Dashboard: http://localhost:5173
- API (Swagger UI when Development): http://localhost:5000/swagger
- MinIO console: http://localhost:9001 (user: `minioadmin`, password: `minioadmin`)

Update sensitive values such as `Jwt__Key` and `OpenAi__ApiKey` in `docker-compose.yml` before sharing the stack. The compose workflow seeds the `ore-media` bucket on first boot.

Apply EF Core migrations against the running Postgres container when needed:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\migrate.ps1 -Action update -Environment Development -Connection "Host=localhost;Port=5432;Database=ore_dev;Username=postgres;Password=postgres"
```

## Migration Tooling

The `scripts` directory provides reusable automation:

- `migrate.ps1`: Core PowerShell script that accepts `-Action`, `-Migration`, `-Environment`, and optional connection strings.
- `migrate-development.bat` / `migrate-staging.bat` / `migrate-production.bat`: Batch wrappers with positional parameters for quick usage on Windows.
- `.github/workflows/migrations.yml`: GitHub Actions workflow to run migrations on demand or when persistence code changes merge to `main`.

Example:

```powershell
# Add migration for Development
dotnet tool restore
powershell -ExecutionPolicy Bypass -File .\scripts\migrate.ps1 -Action add -Migration AddTeamOwnership

# Apply to staging with explicit connections
scripts\migrate-staging.bat update InitialCreate "Host=staging-db;Port=5432;Database=ore_stage;Username=postgres;Password=secret" "staging-redis:6379"
```

## Next Steps

1. Implement authenticated REST endpoints in the API (users, surveys, content lifecycle).
2. Orchestrate background jobs in the worker for content publishing & notifications.
3. Build dashboard pages for admin workflows (survey builder, content approvals, scheduling).
4. Add Docker compose and deployment pipeline wiring.
5. Expand automated test coverage and quality gates.

Track progress by updating the checklist above as milestones are completed.
