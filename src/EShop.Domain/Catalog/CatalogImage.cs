
namespace EShop.Domain.Catalog;


public sealed class CatalogImage
{
    public Guid Id { get; private set; }
    public string Url { get; private set; } = null!;
    public string AltText { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private CatalogImage() { } // EF

    private CatalogImage(string url, string? altText, int sortOrder, bool isPrimary)
    {
        Url = ValidateUrl(url);
        AltText = altText?.Trim() ?? string.Empty;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        Id = Guid.NewGuid();
    }

    public static CatalogImage Create(string url, string? altText, int sortOrder, bool isPrimary = false)
        => new(url, altText, sortOrder, isPrimary);

    public void ChangeAltText(string? altText)
    {
        AltText = altText?.Trim() ?? string.Empty;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    public void ChangeSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "Sort order cannot be negative.");

        SortOrder = sortOrder;
    }

    private static string ValidateUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        
        return url.Trim();
    }
}
