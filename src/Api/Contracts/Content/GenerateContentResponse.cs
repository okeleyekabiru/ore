using System.Collections.Generic;

namespace Ore.Api.Contracts.Content;

public sealed record GenerateContentResponse(
    string Caption,
    IReadOnlyCollection<string> Hashtags,
    string ImageIdea);
