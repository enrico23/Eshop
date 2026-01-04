namespace EShop.Shared.Eventing;

public sealed class ServiceBusConsumerOptions
{
    public string EntityName { get; set; } = default!;
    public int MaxConcurrentCalls { get; set; } = 8;
    public int PrefetchCount { get; set; } = 0;
    public bool AutoComplete { get; set; } = false;
    public string QueueName { get; set; } = string.Empty;
}
