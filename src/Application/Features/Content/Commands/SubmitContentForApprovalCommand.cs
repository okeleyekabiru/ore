using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Content.Commands;

public sealed record SubmitContentForApprovalCommand(Guid ContentId, Guid RequestedBy) : IRequest<Result<Guid>>;

public sealed class SubmitContentForApprovalCommandHandler : IRequestHandler<SubmitContentForApprovalCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public SubmitContentForApprovalCommandHandler(IApplicationDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
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

        content.SubmitForApproval();

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (content.AuthorId is not null)
        {
            await _notificationService.DispatchAsync(
                content.AuthorId.Value,
                NotificationType.ApprovalRequested,
                $"Content {content.Title} requires approval",
                cancellationToken);
        }

        return Result<Guid>.Success(content.Id);
    }
}
