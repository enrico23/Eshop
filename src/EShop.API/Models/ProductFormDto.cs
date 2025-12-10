using EShop.Shared.Representations.Product;

namespace EShop.API.Models;

public class ProductFormDto : ProductUpsertDto
{
    public IFormFile? File { get; set; } = null!;

    public string? ImageAltText { get; set; }
}
