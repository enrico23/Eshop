using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Representations.Category;

public abstract class CategoryUpsertDto
{
    [Required]
    public required string Name { get; set; }

    public CategoryImageDto? Image { get; set; } = null!;
}

public record CategoryImageDto(Guid Id, string Url, string? AltText)
    : BaseImageDto(Id, Url, AltText);

public record BaseImageDto(Guid Id, string Url, string? AltText);