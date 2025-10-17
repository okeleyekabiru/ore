using System;

namespace Ore.Api.Contracts.ContentPipeline;

public sealed record CreateContentPipelineItemRequest(
    Guid? TeamId,
    string Title,
    string? Status,
    string? Channel,
    DateTime? DueOnUtc);