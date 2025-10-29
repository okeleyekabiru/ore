using System;
using System.Collections.Generic;

namespace Ore.Api.Contracts.Content;

public sealed record PendingContentResponse(
    Guid Id,
    string Title,
    string Body,
    string? Caption,
    IReadOnlyCollection<string> Hashtags,
    DateTime SubmittedOnUtc,
    PendingContentAuthorResponse Author
);

public sealed record PendingContentAuthorResponse(
    Guid Id,
    string Name,
    string Email
);