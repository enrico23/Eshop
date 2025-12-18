using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers;


[ApiController]
[Route($"api/messages")]
public class MessagesController : ControllerBase
{
    private readonly ServiceBusClient _client;

    public MessagesController(ServiceBusClient client)
    {
        _client = client;
    }

    [HttpPost("images")]
    public async Task<IActionResult> SendImageMessage(
        [FromBody] SendImageRequest request,
        CancellationToken cancellationToken)
    {
        var sender = _client.CreateSender("images-queue");

        var message = new ServiceBusMessage(request.ImageName)
        {
            MessageId = Guid.NewGuid().ToString(),
            Subject = "image.created"
        };

        await sender.SendMessageAsync(message, cancellationToken);

        return Accepted(new
        {
            Queue = "images-queue",
            request.ImageName
        });
    }
}

public sealed record SendImageRequest(string ImageName);

