using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Product;

public abstract class ProductUpsertDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Description { get; set; } = null!;

    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "SKU can only contain letters, numbers, hyphens, and underscores.")]
    public string Sku { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public int? CategoryId { get; set; }
}
