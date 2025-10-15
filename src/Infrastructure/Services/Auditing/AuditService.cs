using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Domain.Entities;

namespace Ore.Infrastructure.Services.Auditing;

public sealed class AuditService : IAuditService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IApplicationDbContext dbContext, ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogAsync(string actor, string action, string entity, string entityId, string metadata, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog(actor, action, entity, entityId, metadata);
        await _dbContext.AuditLogs.AddAsync(log, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Audit event recorded: {Action} on {Entity} by {Actor}.", action, entity, actor);
    }
}
