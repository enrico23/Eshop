using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Domain.Products;
using EShop.Shared.Domain;

public sealed class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public ProductDescription Description { get; private set; } = null!;


    public Money Price { get; private set; } = null!;

    public StockQuantity Stock { get; private set; } = null!;

    public ProductStatus Status { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category? Category { get; private set; }

    public Product()
    {
    }

    public static Product Create(string name, ProductDescription description, Money price, StockQuantity stock, Guid categoryId)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = ValidateName(name),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            Price = price ?? throw new ArgumentNullException(nameof(price)),
            Stock = stock ?? throw new ArgumentNullException(nameof(stock)),
            CategoryId = ValidateCategoryId(categoryId),
            Status = ProductStatus.Draft
        };
    }

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

    public void ChangeDescription(ProductDescription newDescription)
    {
        Description = newDescription ?? throw new ArgumentNullException(nameof(newDescription));
    }

    public void ChangePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
    }

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
}
