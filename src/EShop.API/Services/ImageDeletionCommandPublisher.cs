using EShop.Shared.Eventing;
using EShop.Shared.Observability.Correlation;
using EShop.Shared.Services;

namespace EShop.API.Services;

public interface IImageDeletionCommandPublisher
{
    Task PublishDeleteAsync(ImageStoragePurpose purpose, string imageUrl, CancellationToken ct);
}

public sealed class ImageDeletionCommandPublisher(
    IEventPublisher eventPublisher,
    ICorrelationIdAccessor correlationIdAccessor) : IImageDeletionCommandPublisher
{
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly ICorrelationIdAccessor _correlationIdAccessor = correlationIdAccessor;

    public async Task PublishDeleteAsync(ImageStoragePurpose purpose, string imageUrl, CancellationToken ct)
    {
        await _eventPublisher.PublishAsync(
            "images-queue",
            new EventEnvelope<DeleteImageRequest>(
                Type: "image.deleted",
                Data: new DeleteImageRequest(purpose, imageUrl),
                CorrelationId: _correlationIdAccessor.CorrelationId,
                Source: "eshop.api"), ct);
    }
}
