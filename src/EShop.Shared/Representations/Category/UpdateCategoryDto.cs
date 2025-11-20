using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Category;

public class UpdateCategoryDto
{
    [Required]
    public required string Name { get; set; }
}
