using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations.Brand;

public class UpdateBrandDto
{
    [Required]
    public required string Name { get; set; }
}
