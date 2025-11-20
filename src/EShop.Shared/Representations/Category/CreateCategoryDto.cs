using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Category;

public class CreateCategoryDto
{
    [Required]
    public required string Name { get; set; }
}
