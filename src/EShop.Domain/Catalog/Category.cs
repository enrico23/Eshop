using EShop.Domain.Exceptions;
using EShop.Domain.Validation;
using System.Collections.Immutable;

namespace EShop.Domain.Catalog;

public sealed class  Category : AuditableEntity, IEntity<Guid>
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public ICollection<Product> Products { get; private set; } = [];

    private readonly List<CatalogImage> _images = [];
    public IReadOnlyCollection<CatalogImage> Images => _images.AsReadOnly();

    public Category() {}

    public static Category Create(string name)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = NameValidation.Validate(name, 200, "Category name")
        };
    }

    public static Category Create(Guid id, string name)
    {
        return new Category
        {
            Id = id,
            Name = NameValidation.Validate(name, 200, "Category name")
        };
    }

    public void ChangeName(string newName) => Name = NameValidation.Validate(newName, 200, "Category name");

    public CatalogImage AddImage(string url, string? altText = null)
    {
        if (_images.Any(i => i.Url == url))
            throw new DomainException("This image is already associated with the category.");

        var nextSortOrder = _images.Count == 0
            ? 0
            : _images.Max(i => i.SortOrder) + 1;

        var isPrimary = !_images.Any(i => i.IsPrimary);

        var image = CatalogImage.Create(url, altText, nextSortOrder, isPrimary);
        _images.Add(image);

        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.SingleOrDefault(i => i.Id == imageId)
            ?? throw new DomainException("Image not found.");

        var wasPrimary = image.IsPrimary;
        _images.Remove(image);

        // if we removed the primary image, optionally promote another one
        if (wasPrimary && _images.Any())
        {
            var firstBySort = _images.OrderBy(i => i.SortOrder).First();
            SetPrimaryImage(firstBySort.Id);
        }
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.SingleOrDefault(i => i.Id == imageId)
            ?? throw new DomainException("Image not found.");

        foreach (var img in _images)
        {
            img.SetPrimary(img.Id == image.Id);
        }
    }

    public void ReorderImages(IReadOnlyList<Guid> orderedIds)
    {
        if (orderedIds.Count != _images.Count ||
            orderedIds.Except(_images.Select(i => i.Id)).Any())
        {
            throw new DomainException("Invalid image ordering.");
        }

        var orderMap = orderedIds
            .Select((id, index) => (id, index))
            .ToDictionary(x => x.id, x => x.index);

        foreach (var img in _images)
        {
            img.ChangeSortOrder(orderMap[img.Id]);
        }
    }

    public void ReplaceImages(IReadOnlyList<CatalogImageSpec> newImages)
    {
        ArgumentNullException.ThrowIfNull(newImages);

        _images.Clear();

        if (newImages.Count == 0)
            return;

        var primaryCount = newImages.Count(i => i.IsPrimary);
        if (primaryCount > 1)
            throw new DomainException("Only one primary image is allowed.");

        CatalogImage? requestedPrimary = null;

        foreach (var spec in newImages)
        {
            var image = AddImage(spec.Url, spec.AltText);

            if (spec.IsPrimary && requestedPrimary is null)
            {
                requestedPrimary = image;
            }
        }

        if (requestedPrimary is not null)
            SetPrimaryImage(requestedPrimary.Id);
    }

    #region Catalog Images

    // abstract in helper and use it on both category and product
    #endregion
}
