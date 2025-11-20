using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Brand;

public class UpdateBrandDto
{
    [Required]
    public required string Name { get; set; }
}
