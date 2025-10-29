using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Ore.Infrastructure.Middleware;

public class AuditContextMiddleware
{
    private readonly RequestDelegate _next;

    public AuditContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditContextProvider auditContextProvider)
    {
        // Extract user ID from JWT claims
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Extract IP address (considering proxies)
        var ipAddress = GetClientIPAddress(context);

        // Set the audit context for this request
        auditContextProvider.SetContext(userId, ipAddress);

        await _next(context);
    }

    private static string GetClientIPAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (proxy scenarios)
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            // Take the first IP in the chain
            return xForwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header (nginx scenarios)
        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        // Fall back to remote IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

public interface IAuditContextProvider
{
    void SetContext(string? userId, string? ipAddress);
    string? UserId { get; }
    string? IPAddress { get; }
}

public class AuditContextProvider : IAuditContextProvider
{
    private static readonly AsyncLocal<AuditContext> _auditContext = new();

    public void SetContext(string? userId, string? ipAddress)
    {
        _auditContext.Value = new AuditContext(userId, ipAddress);
    }

    public string? UserId => _auditContext.Value?.UserId;
    public string? IPAddress => _auditContext.Value?.IPAddress;

    private record AuditContext(string? UserId, string? IPAddress);
}