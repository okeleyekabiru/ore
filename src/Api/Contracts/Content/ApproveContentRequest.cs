using System;

namespace Ore.Api.Contracts.Content;

public sealed record ApproveContentRequest(Guid ContentId, string? Comments = null);