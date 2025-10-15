using System;

namespace Ore.Application.Abstractions.Identity;

public sealed record RefreshTokenResult(string Token, DateTime ExpiresOnUtc);
