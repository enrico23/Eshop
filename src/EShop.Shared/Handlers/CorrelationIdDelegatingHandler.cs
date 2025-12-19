using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Handlers;

public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) 
    : DelegatingHandler
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string ContextItemKey = "CorrelationId";
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue(ContextItemKey, out var value) == true &&
            value is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            if (!request.Headers.Contains(CorrelationIdHeaderName))
            {
                request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, correlationId);
            }

            System.Diagnostics.Activity.Current?.SetTag("business.correlation_id", correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
