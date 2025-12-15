using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EShop.API.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace EShop.API.Tests;

public class UploadImageAsyncTests
{
    private readonly IBlobFileStorageService _blobFileStorage;

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadImageAsyncTests()
    {
        var container = 
            Substitute.For<BlobContainerClient>("UseDevelopmentStorage=true", "dummy-container");

        _blobFileStorage = new BlobFileStorageService(container);
    }

    [Fact]
    public async Task When_File_Null_ThrowsBadHttpRequestException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
              await _blobFileStorage.UploadImageAsync(null!, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenFileIsEmpty_ThrowsBadHttpRequestException()
    {
        // Arrange
        var emptyFile = new FormFile(Stream.Null, 0, 0, "empty", "empty.txt");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
              await _blobFileStorage.UploadImageAsync(emptyFile, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenFileExceedsMaxFielSize_ThrowsBadHttpRequestException()
    {
        // Arrange
        IFormFile tooLargeFile = new FormFile(
            baseStream: new MemoryStream(new byte[1]), // stream content doesn't matter here
            baseStreamOffset: 0,
            length: MaxFileSizeBytes + 1, // force Length > MaxFileSizeBytes
            name: "file",
            fileName: "too-large.bin");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
              await _blobFileStorage.UploadImageAsync(tooLargeFile, CancellationToken.None));

        Assert.NotNull(ex);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, ex.StatusCode);
    }

    [Fact]
    public async Task UploadAsync_WhenContentTypeIsNull_Throws400()
    {
        // Arrange
        var file = FormFileHelpers.CreateFormFile(length: 1, fileName: "test.txt", contentType: null);

        // Act
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(() => _blobFileStorage.UploadImageAsync(file));

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
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(() => _blobFileStorage.UploadImageAsync(file));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Equal("Only image files are allowed.", ex.Message);
    }

    [Fact]
    public async Task UploadAsync_WhenContentTypeIsImage_Succeeds()
    {
        // Arrange
        var container = Substitute.For<BlobContainerClient>();
        var blob = Substitute.For<BlobClient>();

        container
            .CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<BlobContainerInfo>>(null!));

        container
            .GetBlobClient(Arg.Any<string>())
            .Returns(blob);

        blob
            .UploadAsync(
                Arg.Any<Stream>(),
                Arg.Any<BlobUploadOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<BlobContentInfo>>(null!));

        var sut = new BlobFileStorageService(container);

        var file = FormFileHelpers.CreateFormFile(length: 1, fileName: "image.png", contentType: "image/png");

        // Act
        var result = await sut.UploadImageAsync(file, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }
}
