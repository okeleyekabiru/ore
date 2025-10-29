using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;

namespace Ore.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ore Worker started at {Time}", DateTimeOffset.Now);
        
        // Ensure database is up to date
        await EnsureDatabaseAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Worker heartbeat at {Time}", DateTimeOffset.Now);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        
        _logger.LogInformation("Ore Worker stopped at {Time}", DateTimeOffset.Now);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            
            if (dbContext is DbContext context)
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending database migrations", pendingMigrations.Count());
                    await context.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Database migrations completed successfully");
                }
                else
                {
                    _logger.LogInformation("Database is up to date");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure database is up to date");
        }
    }
}
