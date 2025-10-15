using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Llm;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;

namespace Ore.Application.Features.Content.Commands;

public sealed record GenerateContentCommand(Guid TeamId, Guid RequestedBy, string Prompt, string Model)
    : IRequest<Result<Guid>>;

public sealed class GenerateContentCommandHandler : IRequestHandler<GenerateContentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILlmService _llmService;
    private readonly INotificationService _notificationService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GenerateContentCommandHandler(
        IApplicationDbContext dbContext,
        ILlmService llmService,
        INotificationService notificationService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _llmService = llmService;
        _notificationService = notificationService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(GenerateContentCommand request, CancellationToken cancellationToken)
    {
        var team = await _dbContext.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result<Guid>.Failure("Team not found");
        }

        var voiceProfile = team.BrandVoice;
        var response = await _llmService.GeneratePostAsync(request.TeamId, voiceProfile, request.Prompt, cancellationToken);

    var contentItem = new ContentItem(request.TeamId, request.RequestedBy, request.Prompt, response, response);
    contentItem.ApplyBrandVoice(voiceProfile);
        contentItem.MarkGenerated();

        var generationRequest = new ContentGenerationRequest(request.TeamId, request.RequestedBy, request.Prompt, request.Model);
        generationRequest.AttachContent(contentItem, response);

        contentItem.AddDomainEvent(new ContentGeneratedEvent(contentItem.Id, request.TeamId, request.RequestedBy));

    _dbContext.ContentItems.Add(contentItem);
    _dbContext.ContentGenerationRequests.Add(generationRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.DispatchAsync(
            request.RequestedBy,
            NotificationType.ContentGenerated,
            "New content generated",
            cancellationToken);

        return Result<Guid>.Success(contentItem.Id);
    }
}
