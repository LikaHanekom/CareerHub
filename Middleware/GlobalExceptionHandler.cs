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
            
            //database
            DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505" 
                => StatusCodes.Status409Conflict, // Unique constraint violation fallback

            DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23502" 
                => StatusCodes.Status400BadRequest, // Not-null constraint violation fallback

            // 404 NOT FOUND
            JobNotFoundException         => StatusCodes.Status404NotFound,
            CompanyNotFoundException     => StatusCodes.Status404NotFound,
            ApplicantNotFoundException   => StatusCodes.Status404NotFound,
            ApplicationNotFoundException => StatusCodes.Status404NotFound,

            // 409 CONFLICT 
            DuplicateJobListingException => StatusCodes.Status409Conflict,
            DuplicateCompanyException    => StatusCodes.Status409Conflict,
            DuplicateApplicantException  => StatusCodes.Status409Conflict,
            DuplicateApplicationException => StatusCodes.Status409Conflict, 

            //  400 BAD REQUEST 
            InvalidJobStatusException       => StatusCodes.Status400BadRequest,
            InvalidStatusTransitionException => StatusCodes.Status400BadRequest,
            ListingClosedException          => StatusCodes.Status400BadRequest, 

            DbUpdateException dbEx when dbEx.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23503" 
            => StatusCodes.Status404NotFound,

            
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