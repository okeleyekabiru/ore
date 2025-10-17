using System;
using System.Collections.Generic;

namespace Ore.Api.Contracts.ContentPipeline;

public sealed class BulkUpdateContentPipelineStatusRequest
{
    public IReadOnlyCollection<Guid> ItemIds { get; init; } = Array.Empty<Guid>();
    public string Status { get; init; } = string.Empty;
    public DateTime? ScheduledOnUtc { get; init; }
    public string? Reason { get; init; }
    public string? Platform { get; init; }
}
