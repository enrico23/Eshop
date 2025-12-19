using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Observability.Correlation;

public sealed class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor,
    IOptions<CorrelationIdOptions> options) 
    : DelegatingHandler
{
  
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly CorrelationIdOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Items.TryGetValue(_options.ContextItemKey, out var value) == true &&
            value is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            if (!request.Headers.Contains(_options.HeaderName))
            {
                request.Headers.TryAddWithoutValidation(_options.HeaderName, correlationId);
            }

            System.Diagnostics.Activity.Current?.SetTag(_options.ActivityTagName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
