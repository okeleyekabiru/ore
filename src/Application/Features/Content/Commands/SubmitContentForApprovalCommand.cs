using System;
using System.Linq;
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

public sealed record SubmitContentForApprovalCommand(Guid ContentId, Guid RequestedBy) : IRequest<Result<Guid>>;

public sealed class SubmitContentForApprovalCommandHandler : IRequestHandler<SubmitContentForApprovalCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public SubmitContentForApprovalCommandHandler(IApplicationDbContext dbContext, INotificationService notificationService, IAuditService auditService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(SubmitContentForApprovalCommand request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.ContentItems
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == request.ContentId, cancellationToken);

        if (content is null)
        {
            return Result<Guid>.Failure("Content not found");
        }

        // Check if user is Individual - auto-approve their content
        if (content.Author?.Role == RoleType.Individual)
        {
            var autoApproval = new ApprovalRecord(content.Id, request.RequestedBy, ApprovalStatus.Approved, "Auto-approved for individual user");
            content.AddApprovalRecord(autoApproval);
            content.Approve(autoApproval.Id);
            content.AddDomainEvent(new ContentApprovalEvent(content.Id, request.RequestedBy, ApprovalStatus.Approved));

            _dbContext.ApprovalRecords.Add(autoApproval);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Audit log the auto-approval
            await _auditService.LogAsync(
                content.Author?.FullName ?? "System",
                "AUTO_APPROVE",
                nameof(ContentItem),
                content.Id.ToString(),
                $"{{\"contentTitle\":\"{content.Title}\",\"reason\":\"Individual user auto-approval\"}}",
                cancellationToken);

            if (content.AuthorId is not null)
            {
                await _notificationService.DispatchAsync(
                    content.AuthorId.Value,
                    NotificationType.ApprovalDecision,
                    $"Content {content.Title} auto-approved",
                    cancellationToken);
            }

            return Result<Guid>.Success(content.Id);
        }

        // Team users require manager approval
        content.SubmitForApproval();

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Audit log the approval submission
        await _auditService.LogAsync(
            content.Author?.FullName ?? "Unknown User",
            "SUBMIT_FOR_APPROVAL",
            nameof(ContentItem),
            content.Id.ToString(),
            $"{{\"contentTitle\":\"{content.Title}\",\"teamId\":\"{content.TeamId}\"}}",
            cancellationToken);

        // Find team managers/approvers to notify
        var teamManagers = await _dbContext.Users
            .Where(u => u.TeamId == content.TeamId && 
                       (u.Role == RoleType.Admin || 
                        u.Role == RoleType.SocialMediaManager || 
                        u.Role == RoleType.Approver))
            .ToListAsync(cancellationToken);

        foreach (var manager in teamManagers)
        {
            await _notificationService.DispatchAsync(
                manager.Id,
                NotificationType.ApprovalRequested,
                $"Content {content.Title} requires approval from {content.Author?.FullName}",
                cancellationToken);
        }

        if (content.AuthorId is not null)
        {
            await _notificationService.DispatchAsync(
                content.AuthorId.Value,
                NotificationType.ApprovalRequested,
                $"Content {content.Title} submitted for approval",
                cancellationToken);
        }

        return Result<Guid>.Success(content.Id);
    }
}
