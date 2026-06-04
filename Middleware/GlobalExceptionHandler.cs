using CareerHub.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerHub.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(exception, "An exception occurred.");

        var statusCode = exception switch
        {

            DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505" 
            => StatusCodes.Status409Conflict, // 23505 = Unique Violation (e.g., Company name already exists)

            DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23502" 
            => StatusCodes.Status400BadRequest, 
            JobNotFoundException => StatusCodes.Status404NotFound,
            CompanyNotFoundException => StatusCodes.Status404NotFound,
            DuplicateJobListingException => StatusCodes.Status409Conflict,
            DuplicateCompanyException => StatusCodes.Status409Conflict,
            InvalidJobStatusException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception.GetType().Name,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}