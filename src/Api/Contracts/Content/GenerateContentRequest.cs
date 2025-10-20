using System;

namespace Ore.Api.Contracts.Content;

public sealed record GenerateContentRequest(
    Guid BrandId,
    string Topic,
    string Tone,
    string Platform);
