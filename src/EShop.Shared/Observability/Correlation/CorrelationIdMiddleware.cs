using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EShop.Shared.Observability.Correlation;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next, 
    ILogger<CorrelationIdMiddleware> logger,
    IOptions<CorrelationIdOptions> options)
{
    private readonly CorrelationIdOptions _options = options.Value;

    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var id = _options.Policy switch
        {
           CorrelationIdPolicy.PreserveIncoming =>
                GetOrSetCorrelationId(context),
            _ => CreateId()
        };

        context.Items[_options.ContextItemKey] = id;

        Activity.Current?.SetTag(_options.ActivityTagName, id);

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   { "CorrelationId", id }
               }))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[_options.HeaderName] = id.ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    private string CreateId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string GetOrSetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.HeaderName, out var correlationIdValues))
        {
            var incoming = correlationIdValues.FirstOrDefault();
            if (IsValid(incoming))
                return incoming!;
        }

        return CreateId();
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
