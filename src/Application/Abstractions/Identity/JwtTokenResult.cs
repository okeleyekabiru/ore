using System;

namespace Ore.Application.Abstractions.Identity;

public sealed record JwtTokenResult(string Token, DateTime ExpiresOnUtc);
