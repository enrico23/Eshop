using EShop.Domain.Validation;

namespace EShop.Domain.Catalog;

public sealed class Category : AuditableEntity, IEntity<Guid>
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public ICollection<Product> Products { get; private set; } = [];

    private readonly List<CatalogImage> _images = [];
    private readonly CatalogImageCollection _imageCollection;
    public IReadOnlyCollection<CatalogImage> Images => _images.AsReadOnly();

    public Category()
    {
        _imageCollection = new CatalogImageCollection("category", _images);
    }

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

    #region Catalog Images

    public CatalogImage AddImage(string url, string? altText = null) => _imageCollection.Add(url, altText);
    public void RemoveImage(Guid imageId) => _imageCollection.Remove(imageId);
    public void SetPrimaryImage(Guid imageId) => _imageCollection.SetPrimary(imageId);
    public void ReorderImages(IReadOnlyList<Guid> orderedIds) => _imageCollection.Reorder(orderedIds);
    public void ReplaceImages(IReadOnlyList<CatalogImageSpec> newImages) => _imageCollection.Replace(newImages);

    #endregion
}
