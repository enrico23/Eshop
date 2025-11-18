using System.ComponentModel.DataAnnotations;

namespace EShop.API.Representations;

public class CreateBrandDto
{
    [Required]
    public required string Name { get; set; }
}