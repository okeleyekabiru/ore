using System;

namespace Ore.Api.Contracts.ContentPipeline;

public sealed class ContentPipelineItemsQueryParameters
{
    public Guid? TeamId { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? Search { get; init; }
}
