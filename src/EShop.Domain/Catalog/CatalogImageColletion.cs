using EShop.Domain.Exceptions;

namespace EShop.Domain.Catalog;

public sealed class CatalogImageCollection
{
    private readonly List<CatalogImage> _items;
    private readonly string _ownerName;

    public CatalogImageCollection(string ownerName, List<CatalogImage>? items = null)
    {
        _ownerName = ownerName;
        _items = items ?? [];
    }

    public IReadOnlyCollection<CatalogImage> Items => _items.AsReadOnly();

    public CatalogImage Add(string url, string? altText = null)
    {
        if (_items.Any(i => i.Url == url))
            throw new DomainException($"This image is already associated with the {_ownerName}.");

        var nextSortOrder = _items.Count == 0 ? 0 : _items.Max(i => i.SortOrder) + 1;
        var isPrimary = !_items.Any(i => i.IsPrimary);

        var image = CatalogImage.Create(url, altText, nextSortOrder, isPrimary);
        _items.Add(image);
        return image;
    }

    public void Remove(Guid imageId)
    {
        var image = _items.SingleOrDefault(i => i.Id == imageId)
            ?? throw new DomainException("Image not found.");

        var wasPrimary = image.IsPrimary;
        _items.Remove(image);

        if (wasPrimary && _items.Any())
            SetPrimary(_items.OrderBy(i => i.SortOrder).First().Id);
    }

    public void SetPrimary(Guid imageId)
    {
        var image = _items.SingleOrDefault(i => i.Id == imageId)
            ?? throw new DomainException("Image not found.");

        foreach (var img in _items)
            img.SetPrimary(img.Id == image.Id);
    }

    public void Reorder(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count != _items.Count || orderedIds.Except(_items.Select(i => i.Id)).Any())
            throw new DomainException("Invalid image ordering.");

        var map = orderedIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);

        foreach (var img in _items)
            img.ChangeSortOrder(map[img.Id]);
    }

    public void Replace(IReadOnlyList<CatalogImageSpec> newImages)
    {
        ArgumentNullException.ThrowIfNull(newImages);

        _items.Clear();
        if (newImages.Count == 0) return;

        if (newImages.Count(i => i.IsPrimary) > 1)
            throw new DomainException("Only one primary image is allowed.");

        CatalogImage? requestedPrimary = null;
        foreach (var spec in newImages)
        {
            var image = Add(spec.Url, spec.AltText);
            if (spec.IsPrimary && requestedPrimary is null)
                requestedPrimary = image;
        }

        if (requestedPrimary is not null)
            SetPrimary(requestedPrimary.Id);
    }
}
