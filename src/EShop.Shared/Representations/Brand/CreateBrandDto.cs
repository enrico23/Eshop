using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Brand;

public class CreateBrandDto
{
    [Required]
    public required string Name { get; set; }
}