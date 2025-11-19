using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations.Category;

public class UpdateCategoryDto
{
    [Required]
    public required string Name { get; set; }
}
