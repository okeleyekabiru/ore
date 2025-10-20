using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Llm;
using Ore.Application.Abstractions.Messaging;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.Events;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Features.Content.Commands;

public sealed record GenerateContentCommand(
    Guid TeamId,
    Guid RequestedBy,
    string Topic,
    string Tone,
    PlatformType Platform)
    : IRequest<Result<GeneratedContentResult>>;

public sealed class GenerateContentCommandHandler : IRequestHandler<GenerateContentCommand, Result<GeneratedContentResult>>
{
    private static readonly string[] RestrictedKeywords =
    {
        "violence",
        "hate",
        "gambling",
        "exploit",
        "weapon"
    };

    private const string DefaultModelName = "gpt-4.1";

    private readonly IApplicationDbContext _dbContext;
    private readonly ILlmService _llmService;
    private readonly INotificationService _notificationService;

    public GenerateContentCommandHandler(
        IApplicationDbContext dbContext,
        ILlmService llmService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _llmService = llmService;
        _notificationService = notificationService;
    }

    public async Task<Result<GeneratedContentResult>> Handle(GenerateContentCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Topic))
        {
            return Result<GeneratedContentResult>.Failure("Topic is required.");
        }

        var team = await _dbContext.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

        if (team is null)
        {
            return Result<GeneratedContentResult>.Failure("Brand not found.");
        }

        var survey = await _dbContext.BrandSurveys
            .Where(s => s.TeamId == request.TeamId)
            .Include(s => s.Questions)
            .OrderByDescending(s => s.ModifiedOnUtc ?? s.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        BrandSurveySubmission? submission = null;
        if (survey is not null)
        {
            submission = await _dbContext.BrandSurveySubmissions
                .Where(sub => sub.SurveyId == survey.Id)
                .Include(sub => sub.Answers)
                .OrderByDescending(sub => sub.ModifiedOnUtc ?? sub.CreatedOnUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var brandContext = BuildBrandContext(team, survey, submission);
        var platformName = request.Platform.ToString();

        var prompt = BuildPrompt(platformName, request.Topic, request.Tone, brandContext);

        var generation = await _llmService.GenerateContentAsync(request.TeamId, team.BrandVoice, prompt, cancellationToken);

        if (!IsContentAllowed(generation))
        {
            return Result<GeneratedContentResult>.Failure("Generated content failed moderation.");
        }

        await PersistGenerationAsync(request, prompt, generation, team.BrandVoice, cancellationToken);

        return Result<GeneratedContentResult>.Success(generation);
    }

    private static string BuildPrompt(string platform, string topic, string tone, string brandContext)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are an experienced social media strategist.");
        builder.AppendLine($"Create a {platform} post about the topic: {topic.Trim()}.");
        builder.AppendLine("Craft the response as JSON with the following shape:");
        builder.AppendLine("{ \"caption\": string, \"hashtags\": string[], \"imageIdea\": string }");

        if (!string.IsNullOrWhiteSpace(tone))
        {
            builder.AppendLine($"Aim for a tone that feels {tone.Trim()}.");
        }

        if (!string.IsNullOrWhiteSpace(brandContext))
        {
            builder.AppendLine();
            builder.AppendLine("Brand context:");
            builder.AppendLine(brandContext);
        }

        builder.AppendLine();
        builder.AppendLine("Respond with concise caption copy (<= 120 words), 4-6 relevant hashtags, and one imaginative visual direction.");

        return builder.ToString();
    }

    private static string BuildBrandContext(Team team, BrandSurvey? survey, BrandSurveySubmission? submission)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Brand: {team.Name}");

        if (team.BrandVoice is not null)
        {
            builder.AppendLine($"Voice: {team.BrandVoice.Voice}");
            builder.AppendLine($"Tone: {team.BrandVoice.Tone}");
            builder.AppendLine($"Audience: {team.BrandVoice.Audience}");
            if (!string.IsNullOrWhiteSpace(team.BrandVoice.Goals))
            {
                builder.AppendLine($"Goals: {team.BrandVoice.Goals}");
            }
            if (!string.IsNullOrWhiteSpace(team.BrandVoice.Competitors))
            {
                builder.AppendLine($"Competitors: {team.BrandVoice.Competitors}");
            }
            if (team.BrandVoice.Keywords.Any())
            {
                builder.AppendLine($"Keywords: {string.Join(", ", team.BrandVoice.Keywords)}");
            }
        }

        if (survey is not null && submission is not null)
        {
            var questionLookup = survey.Questions.ToDictionary(q => q.Id, q => q.Prompt);
            builder.AppendLine();
            builder.AppendLine("Latest brand survey insights:");

            foreach (var answer in submission.Answers)
            {
                if (questionLookup.TryGetValue(answer.QuestionId, out var prompt))
                {
                    builder.AppendLine($"- {prompt}: {answer.Value}");
                }
            }
        }

        return builder.ToString();
    }

    private static bool IsContentAllowed(GeneratedContentResult generation)
    {
        var combined = string.Join(' ', new[]
        {
            generation.Caption,
            generation.ImageIdea,
            string.Join(' ', generation.Hashtags)
        });

        return RestrictedKeywords.All(keyword => !combined.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private async Task PersistGenerationAsync(
        GenerateContentCommand request,
        string prompt,
        GeneratedContentResult generation,
        BrandVoiceProfile? voiceProfile,
        CancellationToken cancellationToken)
    {
        var caption = generation.Caption?.Trim() ?? string.Empty;
        var title = request.Topic.Trim();
        var hashtags = generation.Hashtags ?? Array.Empty<string>();

        var contentItem = new ContentItem(request.TeamId, request.RequestedBy, title, caption, caption);
        contentItem.UpdateContent(title, caption, caption, hashtags);
        contentItem.ApplyBrandVoice(voiceProfile);
        contentItem.MarkGenerated();

        var generationRequest = new ContentGenerationRequest(request.TeamId, request.RequestedBy, prompt, DefaultModelName);
        var rawResponse = JsonSerializer.Serialize(generation, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        generationRequest.AttachContent(contentItem, rawResponse);

        contentItem.AddDomainEvent(new ContentGeneratedEvent(contentItem.Id, request.TeamId, request.RequestedBy));

        _dbContext.ContentItems.Add(contentItem);
        _dbContext.ContentGenerationRequests.Add(generationRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _notificationService.DispatchAsync(
            request.RequestedBy,
            NotificationType.ContentGenerated,
            "New content generated",
            cancellationToken);
    }
}
