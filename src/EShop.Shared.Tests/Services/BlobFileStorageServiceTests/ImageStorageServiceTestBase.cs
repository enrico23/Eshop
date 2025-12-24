using Azure.Storage.Blobs;
using EShop.Shared.Services;
using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Tests.Services.BlobFileStorageServiceTests;

public abstract class ImageStorageServiceTestBase 
{
    protected ImageStorageServiceOptions Options = new () { 
        MaxUploadSizeBytes = 5242880 
    };

    protected BlobContainerClient Container = Substitute.For<BlobContainerClient>();

    protected BlobClient BlobClient = Substitute.For<BlobClient>();

    protected ImageStorageService CreateSut()
    {
        Container.GetBlobClient(Arg.Any<string>()).Returns(BlobClient);
        return new ImageStorageService(Container, Options);
    }

    protected static IFormFile FakeImage(
        string contentType = "image/png", 
        int length = 1024)
    {
        var stream = new MemoryStream(new byte[length]);
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(length);
        file.FileName.Returns("test.png");
        file.ContentType.Returns(contentType);
        file.OpenReadStream().Returns(stream);
        return file;
    }
}