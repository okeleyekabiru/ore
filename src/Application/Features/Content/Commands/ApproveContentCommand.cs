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

public sealed record ApproveContentCommand(Guid ContentId, Guid ApproverId, string? Comments) : IRequest<Result<Guid>>;

public sealed class ApproveContentCommandHandler : IRequestHandler<ApproveContentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public ApproveContentCommandHandler(IApplicationDbContext dbContext, INotificationService notificationService, IAuditService auditService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(ApproveContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.ContentItems
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content is null)
        {
            return Result<Guid>.Failure("Content not found");
        }

        var approval = new ApprovalRecord(content.Id, request.ApproverId, ApprovalStatus.Approved, request.Comments);
        content.AddApprovalRecord(approval);
        content.Approve(approval.Id);
        content.AddDomainEvent(new ContentApprovalEvent(content.Id, request.ApproverId, ApprovalStatus.Approved));

        _dbContext.ApprovalRecords.Add(approval);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Get approver info for audit log
        var approver = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.ApproverId, cancellationToken);

        // Audit log the approval
        await _auditService.LogAsync(
            approver?.FullName ?? "Unknown Approver",
            "APPROVE",
            nameof(ContentItem),
            content.Id.ToString(),
            $"{{\"contentTitle\":\"{content.Title}\",\"comments\":\"{request.Comments ?? ""}\",\"approverId\":\"{request.ApproverId}\"}}",
            cancellationToken);

        if (content.AuthorId.HasValue)
        {
            await _notificationService.DispatchAsync(
                content.AuthorId.Value,
                NotificationType.ApprovalDecision,
                $"Content {content.Title} approved",
                cancellationToken);
        }

        return Result<Guid>.Success(content.Id);
    }
}
