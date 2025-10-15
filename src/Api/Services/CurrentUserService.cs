using System;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Ore.Api.Common.Exceptions;
using Ore.Application.Abstractions.Identity;

namespace Ore.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "No authenticated user associated with the current request.");
        }

        var identifier = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return identifier is null
            ? throw new ApiException(HttpStatusCode.Unauthorized, "User identifier claim missing.")
            : Guid.Parse(identifier);
    }
}
