using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Exceptions;

public sealed class ConcurrencyExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateConcurrencyException)
        {
            return false;
        }

        const int status = StatusCodes.Status409Conflict;

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = "Concurrency conflict.",
            Detail = "The resource was modified by another request. Refresh and retry.",
            Type = "https://httpstatuses.com/409"
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
