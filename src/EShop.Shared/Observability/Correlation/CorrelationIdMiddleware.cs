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
            _ => CorrelationIdGenerator.CreateId()
        };

        context.Items[_options.ContextItemKey] = id;

        Activity.Current?.SetTag(_options.ActivityTagName, id);

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   {_options.LogFieldName, id }
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

    private string GetOrSetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.HeaderName, out var correlationIdValues))
        {
            var incoming = correlationIdValues.FirstOrDefault();
            if (CorrelationIdValidator.IsValid(incoming, _options))
                return incoming!;
        }

        return CorrelationIdGenerator.CreateId();
    }
}
