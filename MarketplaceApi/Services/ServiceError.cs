using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Services;

public enum ErrorKind { Invalid, Forbidden, NotFound, Conflict }

public class ServiceError(ErrorKind kind, string message) : Exception(message)
{
    public ErrorKind Kind { get; } = kind;

    public static ServiceError Missing(string resource) => new(ErrorKind.NotFound, $"{resource} was not found.");
    public static ServiceError Forbidden() => new(ErrorKind.Forbidden, "You do not have permission for this action.");
    public static ServiceError Conflict(string message) => new(ErrorKind.Conflict, message);
    public static ServiceError Invalid(string message) => new(ErrorKind.Invalid, message);
}

public class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var sqlite = exception as SqliteException ?? exception.InnerException as SqliteException;
        var conflict = exception is DbUpdateConcurrencyException ||
            sqlite is { SqliteErrorCode: 5 or 6 } ||
            sqlite is { SqliteExtendedErrorCode: 2067 or 1555 or 787 };
        var status = exception is ServiceError error
            ? error.Kind switch
            {
                ErrorKind.Invalid => 400,
                ErrorKind.Forbidden => 403,
                ErrorKind.NotFound => 404,
                _ => 409
            }
            : conflict ? 409 : 500;

        if (status == 500)
            logger.LogError(exception, "Unhandled API failure for {Path}", context.Request.Path);
        else
            logger.LogWarning("API request {Path} failed ({Status}): {Message}",
                context.Request.Path, status, exception.Message);

        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    400 => "Invalid request",
                    403 => "Forbidden",
                    404 => "Not found",
                    409 => "State conflict",
                    _ => "An unexpected error occurred"
                },
                Detail = exception is ServiceError ? exception.Message :
                    conflict ? "The data changed or is busy. Reload and retry the operation." : null
            }
        });
    }
}
