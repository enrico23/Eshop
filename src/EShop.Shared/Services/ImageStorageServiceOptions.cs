using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Services;

public sealed class ImageStorageServiceOptions
{
    [Range(1, long.MaxValue)]
    public long MaxUploadSizeBytes { get; init; } = 5 * 1024 * 1024;

    [Required, MinLength(3)]
    public string CustomerContainer { get; init; } = "product-images";

    [Required, MinLength(3)]
    public string UiAssetsContainer { get; init; } = "ui-assets";
}
