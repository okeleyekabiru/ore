using System;
using System.Collections.Generic;

namespace Ore.Api.Contracts.ContentPipeline;

public sealed record ContentPipelineOwnerResponse(Guid? Id, string? Name);

public sealed record ContentPipelineChannelResponse(string Id, string Name);

public sealed record ContentPipelineItemResponse(
    Guid Id,
    Guid TeamId,
    string Title,
    string Status,
    ContentPipelineChannelResponse Channel,
    ContentPipelineOwnerResponse Owner,
    DateTime UpdatedOnUtc,
    DateTime? DueOnUtc,
    DateTime? ScheduledOnUtc);

public sealed record ContentPipelineItemsResponse(
    IReadOnlyCollection<ContentPipelineItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
