using EShop.Shared.Services;
using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Tests.Services.BlobFileStorageServiceTests;

public sealed class UploadImageAsyncTests : ImageStorageServiceTestBase
{
    [Fact]
    public async Task When_File_Null_ThrowsBadHttpRequestException()
    {
        // Act & Assert
        var blobFileStorage = CreateSut();

        var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
              await blobFileStorage.UploadImageAsync(ImageStoragePurpose.CustomerContent, null!, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenFileIsEmpty_ThrowsBadHttpRequestException()
    {
        // Arrange
        var emptyFile = new FormFile(Stream.Null, 0, 0, "empty", "empty.txt");

        // Act & Assert
        var blobFileStorage = CreateSut();
        var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
              await blobFileStorage.UploadImageAsync(ImageStoragePurpose.CustomerContent, emptyFile, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenFileExceedsMaxFielSize_ThrowsBadHttpRequestException()
    {
        // Arrange
        var maxFileSizeBytes = Options.Value?.MaxUploadSizeBytes ?? 5242880;
        IFormFile tooLargeFile = new FormFile(
            baseStream: new MemoryStream(new byte[1]),
            baseStreamOffset: 0,
            length: maxFileSizeBytes + 1, // force Length > MaxFileSizeBytes
            name: "file",
            fileName: "too-large.bin");

        // Act & Assert
        var blobFileStorage = CreateSut();
        var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
              await blobFileStorage.UploadImageAsync(ImageStoragePurpose.CustomerContent, tooLargeFile, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenContentTypeIsNull_Throws400()
    {
        // Arrange
        var file = FormFileHelpers.CreateFormFile(length: 1, fileName: "test.txt", contentType: null);

        // Act
        var blobFileStorage = CreateSut();
        var ex = await Assert.ThrowsAsync<BadRequestException>(() => 
            blobFileStorage.UploadImageAsync(ImageStoragePurpose.CustomerContent, file));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Equal("Only image files are allowed.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    public async Task UploadAsync_WhenContentTypeIsNotImage_Throws400(string contentType)
    {
        // Arrange
        var file = FormFileHelpers.CreateFormFile(length: 1, fileName: "video.mpg", contentType: contentType);

        // Act
        var blobFileStorage = CreateSut();
        var ex = await Assert.ThrowsAsync<BadRequestException>(() 
            => blobFileStorage.UploadImageAsync(ImageStoragePurpose.CustomerContent, file));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Equal("Only image files are allowed.", ex.Message);
    }

    //[Fact]
    //public async Task UploadAsync_WhenContentTypeIsImage_Succeeds()
    //{
    //    // Arrange
    //    BlobContainerClient
    //        .CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: Arg.Any<CancellationToken>())
    //        .Returns(Task.FromResult<Response<BlobContainerInfo>>(null!));

    //    BlobContainerClient
    //        .GetBlobClient(Arg.Any<string>())
    //        .Returns(BlobClient);

    //    BlobClient
    //        .UploadAsync(
    //            Arg.Any<Stream>(),
    //            Arg.Any<BlobUploadOptions>(),
    //            Arg.Any<CancellationToken>())
    //        .Returns(Task.FromResult<Response<BlobContentInfo>>(null!));

    //    var sut = CreateSut();
    //    var fakeImage = FakeImage("image/png", 1);

    //    // Act
    //    var result = await sut.UploadImageAsync(ImageStoragePurpose.CustomerContent, fakeImage, CancellationToken.None);
        
    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.IsType<string>(result);
    //}
}
