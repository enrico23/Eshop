namespace EShop.API.Tests.Services.BlobFileStorageServiceTests;

public sealed class DeleteImageAsyncTests : BlobFileStorageTestBase
{
    [Fact]
    public async Task DeleteImageAsync_WhenImageId_IsEmpty_ThrowsBadHttpRequestException()
    {
        // Arrange
        var blobFileStorageService = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await blobFileStorageService.DeleteImageAsync(string.Empty, CancellationToken.None));
        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }


    [Fact]
    public async Task DeleteImageAsync_WhenCalled_DeletesBlob()
    {
        // Arrange
        var blobFileStorageService = CreateSut();
        var blobName = "test-image.jpg";
        Container.GetBlobClient(blobName).Returns(BlobClient);

        // Act
        await blobFileStorageService.DeleteImageAsync(blobName, CancellationToken.None);

        // Assert
        await BlobClient.Received(1)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, CancellationToken.None);
    }
}
