namespace EShop.Shared.Services.Eventing;

public sealed record EventEnvelope<T>(
    string Type,
    T Data,
    string? Id = null,
    string? CorrelationId = null,
    string? Source = null,
    IDictionary<string, string>? Metadata = null);
