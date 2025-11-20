using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Product;

public class UpdateProductDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int BrandId { get; set; }
}
