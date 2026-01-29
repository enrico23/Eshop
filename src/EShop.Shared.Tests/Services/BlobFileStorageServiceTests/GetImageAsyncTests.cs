using EShop.Shared.Services;

namespace EShop.Shared.Tests.Services.BlobFileStorageServiceTests;

public sealed class GetImageAsyncTests : ImageStorageServiceTestBase
{
    [Fact]
    public async Task GetImageAsync_WhenCalled_ThrowsBadHttpRequestException()
    {
        // Arrange
        var blobFileStorage = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(async () =>
            await blobFileStorage.GetImageAsync(ImageStoragePurpose.CustomerContent, string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task GetImageAsync_WhenBlobDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        BlobClient.ExistsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Response.FromValue(false, null!)));

        var blobFileStorage = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await blobFileStorage.GetImageAsync(ImageStoragePurpose.CustomerContent, "fakename.jpg", CancellationToken.None));
    }

    [Fact]
    public async Task GetImageAsync_WhenBlobExists_ReturnsStreamAndContentType()
    {
        // Arrange
        var existsResponse = Substitute.For<Response>();
        BlobClient.ExistsAsync(Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(Response.FromValue(true, existsResponse)));

        // Prepare download result
        var expectedContentType = "image/png";
        var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

        // BlobDownloadInfo creation via factory
        var downloadInfo = BlobsModelFactory.BlobDownloadInfo(
            content: expectedStream, 
            contentType: expectedContentType);

        var downloadResponse = Substitute.For<Response>();
        BlobClient.DownloadAsync(Arg.Any<CancellationToken>())
                  .Returns(Task.FromResult(Response.FromValue(downloadInfo, downloadResponse)));

        var sut = CreateSut();

        // Act
        var (stream, contentType) = await sut.GetImageAsync(ImageStoragePurpose.CustomerContent, "image.png", CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, stream);
        Assert.Equal(expectedContentType, contentType);
    }
}
