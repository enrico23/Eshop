using EShop.Domain.Websites;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EShop.Functions;

public class Register
{
    private readonly ILogger<Register> _logger;

    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true};

    private readonly string? _connectionString;

    public Register(ILogger<Register> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("WebsiteDb") 
            ?? throw new InvalidOperationException("Connection string invalid or missing.");
    }

    [Function(nameof(Register))]
    [SqlOutput("dbo.Websites", "WebsiteDb")]
    public async Task<Website?> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Website? deserializedWebsite = JsonSerializer.Deserialize<Website>(requestBody, _options);
        
        return deserializedWebsite == null 
            ? null 
            : new Website(Guid.NewGuid(), deserializedWebsite.Url, deserializedWebsite.XPathExpression);
    }
}
