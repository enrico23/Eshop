using Azure.Storage.Blobs;
using NSubstitute;
using EShop.API.Services;
using Microsoft.AspNetCore.Http;

namespace EShop.API.Tests.Services.BlobFileStorageService;

public sealed class DeleteImageAsyncTests
{

    private readonly IBlobFileStorageService _blobFileStorage;

    public DeleteImageAsyncTests()
    {
        var container = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();
        _blobFileStorage = new EShop.API.Services.BlobFileStorageService(container);
    }

    [Fact]
    public async Task DeleteImageAsync_WhenImageId_IsEmpty_ThrowsBadHttpRequestException()
    {
      var ex = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await _blobFileStorage.DeleteImageAsync(string.Empty, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }


    [Fact]
    public async Task DeleteImageAsync_WhenCalled_DeletesBlob()
    {
        // Arrange
        var container = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();
        var blobFileStorageService = new EShop.API.Services.BlobFileStorageService(container);

        var blobName = "test-image.jpg";
        container.GetBlobClient(blobName).Returns(blobClient);

        // Act
        await blobFileStorageService.DeleteImageAsync(blobName, CancellationToken.None);

        // Assert
        await blobClient.Received(1).DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots, null, CancellationToken.None);
    }
}
