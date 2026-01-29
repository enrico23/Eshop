using EShop.Shared.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace EShop.Shared.Eventing;

public sealed record DeleteImageRequest(ImageStoragePurpose Purpose, string Url);

public class ImageDeletedHandler(
    IImageStorageService imageStorageService, 
    ILogger<ImageDeletedHandler> logger) : IEventHandler
{
    private readonly IImageStorageService _imageStorageService = imageStorageService;

    private readonly ILogger<ImageDeletedHandler> _logger = logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static void ImageDeleted() { }

    private static readonly Action<ILogger, string, Exception?> _imageDeletedLog =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1000, nameof(ImageDeleted)),
            "Image Deleted. Url={Url}");

    public string EventType => "image.deleted";

    public async Task ExecuteAsync(JsonElement data, CancellationToken ct)
    {
        var payload = data.Deserialize<DeleteImageRequest>(JsonOptions);

        if (!string.Equals(EventType, "image.deleted", StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid Event Type");
       
        if (payload?.Url is null)
            throw new InvalidOperationException("Missing Data.Url");

        var activity = Activity.Current;
        activity?.SetTag("app.handler", "ImageDeletedHandler");
        activity?.SetTag("app.event_type", "image.deleted");

        activity?.AddEvent(new ActivityEvent("image.delete.start"));

        await _imageStorageService.DeleteImageAsync(payload.Purpose, payload.Url, ct);

        _imageDeletedLog(_logger, payload.Url, null);

        activity?.AddEvent(new ActivityEvent("image.delete.done"));
        activity?.SetTag("app.outcome", "success");
    }
}