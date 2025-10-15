# Migration Scripts

The `scripts` folder contains reusable helpers for managing Entity Framework Core migrations across environments.

## PowerShell entry point

`migrate.ps1` is the main orchestration script. It accepts the following parameters:

| Parameter | Default | Description |
|-----------|---------|-------------|
| `Action` | `update` | Specifies the EF Core command: `add`, `update`, or `bundle`. |
| `Migration` | `InitialCreate` | Migration name or target. Used by `add` and `update`. |
| `Environment` | `Development` | Sets `ASPNETCORE_ENVIRONMENT` and `DOTNET_ENVIRONMENT`. |
| `DatabaseConnection` | *(empty)* | Optional connection string override for `ConnectionStrings:Database`. |
| `RedisConnection` | *(empty)* | Optional connection string override for `ConnectionStrings:Redis`. |

The script invokes the local `dotnet-ef` tool (`dotnet tool run dotnet-ef`), so make sure `.config/dotnet-tools.json` is present (run `dotnet tool restore` if needed).

### Examples

```powershell
# Add a new migration in Development
powershell -ExecutionPolicy Bypass -File .\scripts\migrate.ps1 -Action add -Migration AddTeamOwnership

# Update the Development database using inline connection strings
powershell -ExecutionPolicy Bypass -File .\scripts\migrate.ps1 -Action update -DatabaseConnection "Host=localhost;Port=5432;Database=ore_dev;Username=postgres;Password=postgres" -RedisConnection "localhost:6379"
```

## Environment-specific batch wrappers

Three convenience batch files call `migrate.ps1` with preset environments:

- `migrate-development.bat`
- `migrate-staging.bat`
- `migrate-production.bat`

Each accepts up to four positional arguments:

```
<Action> <Migration> <DatabaseConnection> <RedisConnection>
```

Usage mirrors the PowerShell script while allowing optional overrides per environment.

```bat
:: Apply latest migration to staging with custom connection strings
scripts\migrate-staging.bat update InitialCreate "Host=staging-db;Port=5432;Database=ore_stage;Username=postgres;Password=secret" "staging-redis:6379"

:: Add a new migration in production (dry run via bundle)
scripts\migrate-production.bat bundle
```

## GitHub Actions integration

The repository includes `.github/workflows/migrations.yml`, which invokes the PowerShell script during manual dispatch or when persistence-layer code changes on `main`. Configure per-environment secrets (`DB_CONNECTION`, `REDIS_CONNECTION`) in the corresponding GitHub environments before running the workflow.
