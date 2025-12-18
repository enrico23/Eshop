using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using System.Diagnostics;

namespace EShop.API.Exceptions;

public class FallbackExceptionHandler : IExceptionHandler
{
    private readonly ILogger<FallbackExceptionHandler> _logger;

    public FallbackExceptionHandler(ILogger<FallbackExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var activity = httpContext.Features.Get<IHttpActivityFeature>()?.Activity ?? Activity.Current;

        _logger.LogError(
            exception,
            "Unhandled exception. TraceId={TraceId} SpanId={SpanId} RequestId={RequestId} Path={Path}",
            activity?.TraceId.ToString(),
            activity?.SpanId.ToString(),
            httpContext.TraceIdentifier,
            httpContext.Request.Path.Value);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await Results.Problem(
            title: "Internal Server Error",
            statusCode: StatusCodes.Status500InternalServerError,
            type: "https://httpstatuses.com/500",
            detail: "An unexpected error occurred."
        ).ExecuteAsync(httpContext);

        return true;
    }
}
