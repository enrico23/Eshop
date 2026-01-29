using Azure.Storage.Blobs;
using EShop.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Tests.Services.BlobFileStorageServiceTests;

public abstract class ImageStorageServiceTestBase 
{
    //protected ImageStorageServiceOptions Options = new () { 
    //    MaxUploadSizeBytes = 5242880,
    //    CustomerContainer = "product-images",
    //    UiAssetsContainer = "ui-assets"
    //};

    protected IBlobContainerResolver Containers = Substitute.For<IBlobContainerResolver>();
    // protected BlobContainerClient BlobContainerClient = Substitute.For<BlobContainerClient>();

    protected BlobClient BlobClient = Substitute.For<BlobClient>();

    protected IOptions<ImageStorageServiceOptions> Options = Substitute.For<IOptions<ImageStorageServiceOptions>>();

    protected ImageStorageService CreateSut()
    {
        // BlobContainerClient.GetBlobClient(Arg.Any<string>()).Returns(BlobClient);
        return new ImageStorageService(Containers, Options);
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