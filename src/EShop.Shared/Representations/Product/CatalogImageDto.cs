namespace EShop.Shared.Representations.Product;

public sealed class CatalogImageDto
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public string? AltText { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }
}
