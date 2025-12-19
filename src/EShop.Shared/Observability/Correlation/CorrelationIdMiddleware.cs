using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EShop.Shared.Observability.Correlation;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string ContextItemKey = "CorrelationId";

    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrSetCorrelationId(context);

        context.Items[ContextItemKey] = correlationId;

        Activity.Current?.SetTag("business.correlation_id", correlationId);

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   { "CorrelationId", correlationId }
               }))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeaderName] = correlationId.ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    private object GetOrSetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdValues))
        {
            var incoming = correlationIdValues.FirstOrDefault();
            if (IsValid(incoming))
                return incoming!;
        }

        return Guid.NewGuid().ToString("N");
    }

    private static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length is < 8 or > 64)
            return false;

        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c) && c is not '-' and not '_' and not '.')
                return false;
        }

        return true;
    }
}
