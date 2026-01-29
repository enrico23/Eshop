using EShop.Shared.Services;
using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Tests.Services.BlobFileStorageServiceTests;

public sealed class DeleteImageAsyncTests : ImageStorageServiceTestBase
{
    [Fact]
    public async Task DeleteImageAsync_WhenImageId_IsEmpty_ThrowsBadHttpRequestException()
    {
        // Arrange
        var blobFileStorageService = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
            await blobFileStorageService.DeleteImageAsync(ImageStoragePurpose.CustomerContent, string.Empty, CancellationToken.None));
        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }


    //[Fact]
    //public async Task DeleteImageAsync_WhenCalled_DeletesBlob()
    //{
    //    // Arrange
    //    var blobFileStorageService = CreateSut();
    //    var blobName = "test-image.jpg";
    //    BlobContainerClient.GetBlobClient(blobName).Returns(BlobClient);

    //    // Act
    //    await blobFileStorageService.DeleteImageAsync(ImageStoragePurpose.CustomerContent, blobName, CancellationToken.None);

    //    // Assert
    //    await BlobClient.Received(1)
    //        .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, CancellationToken.None);
    //}
}
