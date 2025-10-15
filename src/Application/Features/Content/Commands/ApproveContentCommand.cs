using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public ApproveContentCommandHandler(IApplicationDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
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
