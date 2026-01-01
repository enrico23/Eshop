namespace EShop.Shared.Eventing;

public sealed class ServiceBusPublisherOptions
{
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromDays(1);
    public int MaxCachedSenders { get; init; } = 64;
}
