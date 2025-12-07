using EShop.Shared.Representations.Product;
using System.ComponentModel.DataAnnotations;

namespace EShop.API.Models;

public class CreateProductFormDto : ProductUpsertDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    public string? ImageAltText { get; set; }
}
