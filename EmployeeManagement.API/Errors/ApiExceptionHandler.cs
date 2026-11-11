using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isConstraintViolation = exception is DbUpdateException
        {
            InnerException: SqliteException { SqliteErrorCode: 19 }
        };
        var status = isConstraintViolation
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;

        _logger.LogError(
            exception,
            "Request failed with status {StatusCode} for {Path}",
            status,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = status;

        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = isConstraintViolation ? "Conflict" : "An unexpected error occurred.",
                Detail = isConstraintViolation
                    ? "A record with the same unique value already exists."
                    : "The server could not complete the request.",
                Instance = httpContext.Request.Path
            }
        });

        return true;
    }
}
