using EShop.API.Services;

namespace EShop.API.Tests.Services.BlobFileStorageServiceTests;

public sealed class GetImageAsyncTests
{
    [Fact]
    public async Task GetImageAsync_WhenCalled_ThrowsBadHttpRequestException()
    {
        // Arrange
        var sut = new BlobFileStorageService(Substitute.For<BlobContainerClient>());

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await sut.GetImageAsync(string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task GetImageAsync_WhenBlobDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var container = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();

        container.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        blobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(false, null)));

        var blobFileStorage = new BlobFileStorageService(container);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await blobFileStorage.GetImageAsync("fakename.jpg", CancellationToken.None));
    }

    [Fact]
    public async Task GetImageAsync_WhenBlobExists_ReturnsStreamAndContentType()
    {
        // Arrange
        var container = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();
        container.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        var existsResponse = Substitute.For<Response>();
        blobClient.ExistsAsync(Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(Response.FromValue(true, existsResponse)));

        // Prepare download result
        var expectedContentType = "image/png";
        var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

        // BlobDownloadInfo creation via factory
        var downloadInfo = BlobsModelFactory.BlobDownloadInfo(
            content: expectedStream, contentType: expectedContentType
            );

        var downloadResponse = Substitute.For<Response>();
        blobClient.DownloadAsync(Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(Response.FromValue(downloadInfo, downloadResponse)));

        var sut = new BlobFileStorageService(container);

        // Act
        var (stream, contentType) = await sut.GetImageAsync("image.png", CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, stream);
        Assert.Equal(expectedContentType, contentType);
    }
}
