using System.Collections.Generic;

namespace Ore.Api.Tests.Integration;

internal sealed record ApiResponse<T>(T? Data, bool Success, string? Message, IReadOnlyCollection<string> Errors);
