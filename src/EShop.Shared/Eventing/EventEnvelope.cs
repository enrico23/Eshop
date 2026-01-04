using System.Text.Json;

namespace EShop.Shared.Eventing;

public sealed record EventEnvelope<T>(
    string Type,
    T Data,
    string? Id = null,
    string? CorrelationId = null,
    string? Source = null,
    IDictionary<string, string>? Metadata = null);

public sealed record EventEnvelope(
        string? Type, 
        JsonElement Data,
        string? Id = null,
        string? CorrelationId = null,
        string? Source = null,
        IDictionary<string, string>? Metadata = null
    );