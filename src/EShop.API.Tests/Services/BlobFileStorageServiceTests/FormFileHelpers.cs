using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace EShop.API.Tests;

public static class FormFileHelpers
{
    public static IFormFile CreateFormFile(long length, string fileName, string? contentType)
    {
        // Stream content is irrelevant for these tests; avoid large allocations.
        var stream = new MemoryStream(new byte[1]);

        var file = new FormFile(stream, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary()
        };

        if (contentType is not null)
        {
            file.Headers[HeaderNames.ContentType] = contentType;
        }

        return file;
    }
}