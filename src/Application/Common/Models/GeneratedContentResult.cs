using System.Collections.Generic;

namespace Ore.Application.Common.Models;

public sealed record GeneratedContentResult(
    string Caption,
    IReadOnlyCollection<string> Hashtags,
    string ImageIdea,
    string RawText);
