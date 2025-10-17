using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Application.Features.Content.Common;
using Ore.Application.Features.Content.Queries;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Features.Content.Commands;

public sealed record CreateContentItemCommand(
    Guid ActorId,
    Guid? TeamId,
    string Title,
    ContentStatus Status,
    DateTime? DueOnUtc,
    PlatformType? Platform) : IRequest<Result<ContentPipelineItemDto>>;

public sealed class CreateContentItemCommandHandler : IRequestHandler<CreateContentItemCommand, Result<ContentPipelineItemDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateContentItemCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ContentPipelineItemDto>> Handle(CreateContentItemCommand request, CancellationToken cancellationToken)
    {
        var trimmedTitle = request.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedTitle))
        {
            return Result<ContentPipelineItemDto>.Failure("Title is required.");
        }

        var author = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.ActorId, cancellationToken);

        if (author is null)
        {
            return Result<ContentPipelineItemDto>.Failure("Requesting user could not be found.");
        }

        var teamId = request.TeamId ?? author.TeamId;
        if (!teamId.HasValue || teamId.Value == Guid.Empty)
        {
            return Result<ContentPipelineItemDto>.Failure("A valid team could not be resolved for the new content item.");
        }

        var content = new ContentItem(teamId.Value, request.ActorId, trimmedTitle, trimmedTitle, caption: null);

        if (request.Status == ContentStatus.Generated)
        {
            content.MarkGenerated();
        }
        else if (request.Status == ContentStatus.PendingApproval)
        {
            content.SubmitForApproval();
        }
        else if (request.Status == ContentStatus.Scheduled)
        {
            if (request.DueOnUtc is null)
            {
                return Result<ContentPipelineItemDto>.Failure("A publish window is required when scheduling content.");
            }

            if (request.Platform is null)
            {
                return Result<ContentPipelineItemDto>.Failure("A channel is required when scheduling content.");
            }

            try
            {
                var window = PublishingWindow.Create(request.DueOnUtc.Value);
                var distribution = new ContentDistribution(content.Id, request.Platform.Value, window);
                content.Schedule(distribution);
            }
            catch (ArgumentException ex)
            {
                return Result<ContentPipelineItemDto>.Failure(ex.Message);
            }
        }
        else if (request.Status != ContentStatus.Draft)
        {
            return Result<ContentPipelineItemDto>.Failure("Unsupported status for new content items.");
        }

        _dbContext.ContentItems.Add(content);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var persisted = await _dbContext.ContentItems
            .AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.Distributions)
            .FirstOrDefaultAsync(item => item.Id == content.Id, cancellationToken);

        if (persisted is null)
        {
            return Result<ContentPipelineItemDto>.Failure("Content item could not be loaded after creation.");
        }

        var dto = ContentPipelineMapper.MapToDto(persisted);
        return Result<ContentPipelineItemDto>.Success(dto);
    }
}