using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace EShop.Shared.Observability.Correlation;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}

public sealed class CorrelationIdAccessor(
    IHttpContextAccessor httpContextAccessor,
    IOptions<CorrelationIdOptions> options) : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;
    private readonly CorrelationIdOptions _options = options.Value;

    public string? CorrelationId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx is not null &&
                ctx.Items.TryGetValue(_options.ContextItemKey, out var value) &&
                value is string s)
            {
                return s;
            }

            // Optional fallback: if you set the tag on Activity, you can read it here too.
            // This helps in some non-controller code paths, but do not rely on it exclusively.
            var tag = Activity.Current?.GetTagItem(_options.ActivityTagName)?.ToString();
            return string.IsNullOrWhiteSpace(tag) ? null : tag;
        }
    }
}
