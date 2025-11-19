using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations.Brand;

public class CreateBrandDto
{
    [Required]
    public required string Name { get; set; }
}