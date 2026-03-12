using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EShop.Functions;

public class Register
{
    private readonly ILogger<Register> _logger;

    public Register(ILogger<Register> logger)
    {
        _logger = logger;
    }

    [Function("Register")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}

public record Website(Guid Id, string Url, string? XPathExpression);