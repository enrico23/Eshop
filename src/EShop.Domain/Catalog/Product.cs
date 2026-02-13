using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Domain.Products;
using EShop.Domain.Validation;
using EShop.Shared.Domain;

public sealed class Product : AuditableEntity, IEntity<Guid>
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public ProductDescription Description { get; private set; } = null!;

    public Money Price { get; private set; } = null!;

    public StockQuantity Stock { get; private set; } = null!;

    public ProductStatus Status { get; private set; }

    public Sku Sku { get; private set; } = null!;

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }
    
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private readonly List<CatalogImage> _images = new();
    private readonly CatalogImageCollection _imageCollection;
    public IReadOnlyCollection<CatalogImage> Images => _images.AsReadOnly();

    public Product()
    {
        _imageCollection = new CatalogImageCollection("product", _images);
    }

    public static Product Create(string name, ProductDescription description, Money price, StockQuantity stock, Guid categoryId, Sku sku)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = ValidateName(name),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            Price = price ?? throw new ArgumentNullException(nameof(price)),
            Stock = stock ?? throw new ArgumentNullException(nameof(stock)),
            CategoryId = ValidateCategoryId(categoryId),
            Status = ProductStatus.Draft,
            Sku = sku ?? throw new ArgumentNullException(nameof(sku))
        };
    }

    #region Name

    public void ChangeName(string newName) => Name = ValidateName(newName);

    public static string ValidateName(string value) =>
         NameValidation.Validate(value, 200, "Product name");

    #endregion

    #region Description

    public void ChangeDescription(ProductDescription newDescription) =>
        Description = newDescription ?? throw new ArgumentNullException(nameof(newDescription));

    #endregion

    #region Price

    public void ChangePrice(Money newPrice) => 
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));

    #endregion 

    #region Product Stock

    public void IncreaseStock(int amount) =>
        Stock = Stock.Increase(amount);
    
    public void DecreaseStock(int amount) => 
        Stock = Stock.Decrease(amount);

    public void ChangeStock(StockQuantity productQuantity) =>
        Stock = productQuantity;

    #endregion

    #region Product Status

    public void Publish()
    {
        if (Status == ProductStatus.Archived)
            throw new DomainException("Cannot publish an archived product.");

        Status = ProductStatus.Active;
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
    }

    public void Deactivate()
    {
        if (Status == ProductStatus.Archived)
            throw new DomainException("Cannot deactivate an archived product.");

        Status = ProductStatus.Inactive;
    }

    public void Draft()
    {
        if (Status == ProductStatus.Archived)
            throw new DomainException("Cannot draft an archived product.");

        Status = ProductStatus.Draft;
    }

    #endregion

    #region Product Sku

    public void ChangeSku(Sku newSku) =>
        Sku = newSku ?? throw new ArgumentNullException(nameof(newSku));
    
    #endregion

    #region category

    public void ChangeCategory(Guid categoryId) =>
        CategoryId = ValidateCategoryId(categoryId);

    private static Guid ValidateCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required.", nameof(categoryId));

        return categoryId;
    }

    #endregion

    #region Product Images

    public CatalogImage AddImage(string url, string? altText = null) => _imageCollection.Add(url, altText);
    public void RemoveImage(Guid imageId) => _imageCollection.Remove(imageId);
    public void SetPrimaryImage(Guid imageId) => _imageCollection.SetPrimary(imageId);
    public void ReorderImages(IReadOnlyList<Guid> orderedIds) => _imageCollection.Reorder(orderedIds);
    public void ReplaceImages(IReadOnlyList<CatalogImageSpec> newImages) => _imageCollection.Replace(newImages);

    #endregion
}
