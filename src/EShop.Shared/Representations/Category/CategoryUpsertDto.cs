using EShop.Shared.Representations.Product;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Category;

public abstract class CategoryUpsertDto
{
    [Required]
    public required string Name { get; set; }

    public ICollection<CatalogImageDto>? Images { get; set; }
}