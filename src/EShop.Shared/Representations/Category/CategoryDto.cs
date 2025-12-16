namespace EShop.Shared.Representations.Category;

public sealed class CategoryDto : BaseEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
