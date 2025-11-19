using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations.Category;

public class CreateCategoryDto
{
    [Required]
    public required string Name { get; set; }
}
