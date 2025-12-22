namespace EShop.Shared.Observability.Correlation;

public enum CorrelationIdPolicy
{
    /// <summary>
    /// Always generate a new correlation id for every inbound request.
    /// Use this at the edge (untrusted callers, browsers).
    /// </summary>
    GenerateAlways,

    /// <summary>
    /// Preserve the incoming correlation id if present/valid; otherwise generate one.
    /// Use this for downstream services.
    /// </summary>
    PreserveIncoming
}

public sealed class CorrelationIdOptions
{
    /// <summary>
    /// HTTP header used to carry the correlation id between services.
    /// This is the on-the-wire representation and should be consistent
    /// across gateways, APIs, and clients.
    /// </summary>
    public string HeaderName { get; set; } = "X-Correlation-ID";

    /// <summary>
    /// Key used to store the resolved correlation id in HttpContext.Items.
    /// This value is internal to the request pipeline and is not a header name.
    /// </summary>
    public string ContextItemKey { get; set; } = "RequestId";

    /// <summary>
    /// Name of the OpenTelemetry Activity tag used to attach the correlation id
    /// to the current span. This enables correlation id visibility in traces
    /// without affecting distributed trace propagation.
    /// </summary>
    public string ActivityTagName { get; set; } = "business.request_id";

    /// <summary>
    /// Logical field name used when enriching logs with the correlation id.
    /// This should align with logging conventions and structured log schemas
    /// used by the application or organization.
    /// </summary>
    public string LogFieldName { get; set; } = "request_id";

    /// <summary>
    /// Determines how the middleware handles incoming correlation ids.
    /// PreserveIncoming will accept a valid incoming id and generate one only
    /// when missing or invalid.
    /// </summary>
    public CorrelationIdPolicy Policy { get; set; } = CorrelationIdPolicy.PreserveIncoming;

    /// <summary>
    /// Minimum allowed length for an incoming correlation id value.
    /// Used as a validation guardrail to reject malformed or suspicious input.
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    /// Maximum allowed length for an incoming correlation id value.
    /// Used as a validation guardrail to prevent header abuse and log pollution.
    /// </summary>
    public int MaxLength { get; set; } = 64;
}