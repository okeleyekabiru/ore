using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;

namespace Ore.Application.Features.Content.Commands;

public sealed record RejectContentCommand(Guid ContentId, Guid ApproverId, string Reason) : IRequest<Result<Guid>>;

public sealed class RejectContentCommandHandler : IRequestHandler<RejectContentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public RejectContentCommandHandler(IApplicationDbContext dbContext, INotificationService notificationService, IAuditService auditService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(RejectContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.ContentItems
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content is null)
        {
            return Result<Guid>.Failure("Content not found");
        }

        var approval = new ApprovalRecord(content.Id, request.ApproverId, ApprovalStatus.Rejected, request.Reason);
        content.AddApprovalRecord(approval);
        content.Reject(approval.Id);
        content.AddDomainEvent(new ContentApprovalEvent(content.Id, request.ApproverId, ApprovalStatus.Rejected));

        _dbContext.ApprovalRecords.Add(approval);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Get approver info for audit log
        var approver = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.ApproverId, cancellationToken);

        // Audit log the rejection
        await _auditService.LogAsync(
            approver?.FullName ?? "Unknown Approver",
            "REJECT",
            nameof(ContentItem),
            content.Id.ToString(),
            $"{{\"contentTitle\":\"{content.Title}\",\"reason\":\"{request.Reason}\",\"approverId\":\"{request.ApproverId}\"}}",
            cancellationToken);

        if (content.AuthorId.HasValue)
        {
            await _notificationService.DispatchAsync(
                content.AuthorId.Value,
                NotificationType.ApprovalDecision,
                $"Content {content.Title} rejected: {request.Reason}",
                cancellationToken);
        }

        return Result<Guid>.Success(content.Id);
    }
}
