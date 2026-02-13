using EShop.Domain.Exceptions;
using EShop.Domain.Validation;

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

    public void AddImage(string url, string? altText = null)
    {
        if (_images.Any(i => i.Url == url))
            throw new DomainException("This image is already associated with the category.");

        var nextSortOrder = _images.Count == 0
            ? 0
            : _images.Max(i => i.SortOrder) + 1;

        var isPrimary = !_images.Any(i => i.IsPrimary);

        var image = CatalogImage.Create(url, altText, nextSortOrder, isPrimary);
        _images.Add(image);
    }

    #region Catalog Images

    // abstract in helper and use it on both category and product
    #endregion
}
