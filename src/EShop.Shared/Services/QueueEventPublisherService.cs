using Azure.Messaging.ServiceBus;

namespace EShop.Shared.Services;

public interface IQueueEventPublisherService
{
    Task<bool> SendDeleteImageMessageAsync(string imageName, CancellationToken ct);
}

public sealed class QueueEventPublisherService : IQueueEventPublisherService, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public QueueEventPublisherService(ServiceBusClient serviceBusClient)
    {
       _sender = serviceBusClient.CreateSender("images-queue");
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }

    public async Task<bool> SendDeleteImageMessageAsync(string imageName, CancellationToken ct)
    {

        var message = new ServiceBusMessage(imageName)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = "image.deleted"
        };

        await _sender.SendMessageAsync(message, ct);

        return true;
    }
}
