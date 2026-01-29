using AutoFixture;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EShop.Shared.Testing;

public sealed class FormFileCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Register<IFormFile>(() =>
        {
            var content = "test-content";
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            // Important: stream position must be at 0
            stream.Position = 0;

            var file = new FormFile(stream, 0, stream.Length, name: "file", fileName: "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain",
                ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = "\"test.txt\""
                }.ToString()
            };

            return file;
        });
    }
}