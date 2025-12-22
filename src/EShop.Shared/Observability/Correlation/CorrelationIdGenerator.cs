namespace EShop.Shared.Observability.Correlation;

public static class CorrelationIdGenerator
{
    public static string CreateId() => Guid.NewGuid().ToString("N");
}
