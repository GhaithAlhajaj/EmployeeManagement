using System.Security.Claims;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Persistence;

namespace EmployeeManagement.API.Middleware;

public sealed class AuditLoggingMiddleware
{
    private static readonly string[] WriteMethods = ["POST", "PUT", "PATCH", "DELETE"];
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (!WriteMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase) ||
            context.Response.StatusCode is < 200 or >= 300 ||
            context.Request.Path.StartsWithSegments("/api/auth/login"))
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            database.AuditLogs.Add(new AuditLog
            {
                ActorId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirstValue("sub"),
                ActorEmail = context.User.FindFirstValue(ClaimTypes.Email)
                    ?? context.User.FindFirstValue("email")
                    ?? "unknown",
                Method = context.Request.Method,
                Path = context.Request.Path.Value ?? string.Empty,
                StatusCode = context.Response.StatusCode,
                TimestampUtc = DateTimeOffset.UtcNow
            });
            await database.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to persist audit record for {Path}", context.Request.Path);
        }
    }
}
