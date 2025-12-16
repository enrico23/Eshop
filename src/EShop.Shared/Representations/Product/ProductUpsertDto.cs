using EShop.Shared.Domain;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Product;

public abstract class ProductUpsertDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Stock must be greater than 0.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public Guid? CategoryId { get; set; }


    [Required(ErrorMessage = "Please select a product status.")]
    public ProductStatus Status{ get; set; }

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50)]
    public string Sku { get; set; } = null!;

    public ICollection<ProductImageDto>? Images { get; set; }
}
