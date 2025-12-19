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
    public string HeaderName { get; set; } = "X-Request-Id";
    public string ContextItemKey { get; set; } = "RequestId";
    public string ActivityTagName { get; set; } = "business.request_id";
    public string LogFieldName { get; set; } = "request_id";

    public CorrelationIdPolicy Policy { get; set; } = CorrelationIdPolicy.PreserveIncoming;

    // Validation guardrails
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 64;
}