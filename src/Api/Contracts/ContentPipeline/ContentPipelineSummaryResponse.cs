using System;

namespace Ore.Api.Contracts.ContentPipeline;

public sealed record ContentPipelineSummaryResponse(string Status, int Count);
