using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations;

public class UpdateBrandDto
{
    [Required]
    public required string Name { get; set; }
}
