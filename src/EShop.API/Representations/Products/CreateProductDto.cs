using System.ComponentModel.DataAnnotations;

namespace EShop.Api.Representations.Products;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
    
    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int BrandId { get; set; }
}
