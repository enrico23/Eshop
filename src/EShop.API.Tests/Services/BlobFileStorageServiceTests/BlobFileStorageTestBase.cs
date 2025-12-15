using EShop.API.Models;

namespace EShop.API.Tests.Services.BlobFileStorageServiceTests;

public abstract class BlobFileStorageTestBase 
{
    protected BlobFileStorageServiceOptions Options = new () { 
        MaxUploadSizeBytes = 5242880 
    };

    protected BlobContainerClient Container = Substitute.For<BlobContainerClient>();

    protected BlobClient BlobClient = Substitute.For<BlobClient>();

    protected BlobFileStorageService CreateSut()
    {
        Container.GetBlobClient(Arg.Any<string>()).Returns(BlobClient);
        return new BlobFileStorageService(Container, Options);
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