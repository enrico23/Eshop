using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Domain.Products;
using EShop.Shared.Domain;

public sealed class Product : AuditableEntity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public ProductDescription Description { get; private set; } = null!;

    public Money Price { get; private set; } = null!;

    public StockQuantity Stock { get; private set; } = null!;

    public ProductStatus Status { get; private set; }

    public Sku Sku { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    public Product()
    {
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

    public void ChangeName(string newName)
    {
        Name = ValidateName(newName);
    }

    public static string ValidateName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        _ = value.Trim();

        if (value.Length > 200)
            throw new ArgumentException("Product name is limited to 200 characters", nameof(value));

        return value;
    }

    #endregion

    #region Description

    public void ChangeDescription(ProductDescription newDescription)
    {
        Description = newDescription ?? throw new ArgumentNullException(nameof(newDescription));
    }

    #endregion

    #region Price

    public void ChangePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
    }

    #endregion 

    #region Product Stock

    public void IncreaseStock(int amount)
    {
        Stock = Stock.Increase(amount);
    }

    public void DecreaseStock(int amount)
    {
        Stock = Stock.Decrease(amount);
    }


    public void ChangeStock(StockQuantity productQuantity)
    {
        Stock = productQuantity;
    }

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

    public void ChangeSku(Sku newSku)
    {
        Sku = newSku ?? throw new ArgumentNullException(nameof(newSku));
    }

    #endregion

    #region category

    public void ChangeCategory(Guid categoryId)
    {
        CategoryId = ValidateCategoryId(categoryId);
    }

    private static Guid ValidateCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required.", nameof(categoryId));

        return categoryId;
    }

    #endregion

    #region Product Images

    public ProductImage AddImage(string url, string? altText = null)
    {
        if (_images.Any(i => i.Url == url))
            throw new DomainException("This image is already associated with the product.");

        var nextSortOrder = _images.Count == 0
            ? 0
            : _images.Max(i => i.SortOrder) + 1;

        var isPrimary = !_images.Any(i => i.IsPrimary);

        var image = ProductImage.Create(url, altText, nextSortOrder, isPrimary);
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

    public void ReplaceImages(IReadOnlyList<ProductImageSpec> newImages)
    {
        ArgumentNullException.ThrowIfNull(newImages);

        _images.Clear();

        if (newImages.Count == 0)
            return;

        var primaryCount = newImages.Count(i => i.IsPrimary);
        if (primaryCount > 1)
            throw new DomainException("Only one primary image is allowed.");

        ProductImage? requestedPrimary = null;

        foreach (var spec in newImages)
        {
            var image = AddImage(spec.Url, spec.AltText);

            if (spec.IsPrimary && requestedPrimary is null)
            {
                requestedPrimary = image;
            }
        }

        if (requestedPrimary is not null)
        {
            SetPrimaryImage(requestedPrimary.Id);
        }
    }

    #endregion
}

public sealed record ProductImageSpec(
    string Url,
    string? AltText,
    bool IsPrimary);