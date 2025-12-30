using Microsoft.AspNetCore.Http;
using System.Text;

namespace EShop.API.Tests.Services.ProductServiceTests;

public static class FormFileHelpers
{
    public static IFormFile CreateFakeFormFile(
    string fileName = "test.png",
    string contentType = "image/png",
    string contents = "fake-bytes")
    {
        var bytes = Encoding.UTF8.GetBytes(contents);
        var stream = new MemoryStream(bytes);

        return new FormFile(
            baseStream: stream,
            baseStreamOffset: 0,
            length: bytes.Length,
            name: "file",
            fileName: fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
