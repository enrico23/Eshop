using EShop.Domain;

public sealed class Product
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public int Stock { get; private set; }

    public Guid CategoryId { get; private set; }

   
    public Category? Category { get; private set; }

  
    protected Product()
    {
    }

    public static Product Create(string name, string? description, 
        decimal price, int stock, Guid categoryId)
    {
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = ValidateName(name),
            Description = description,
            Price = ValidatePrice(price),
            Stock = stock,
            CategoryId = ValidateCategoryId(categoryId)
        };
    }

    public void ChangeName(string newName)
    {
        Name = ValidateName(newName);
    }

    public void ChangeDescription(string? newDescription)
    {
        Description = newDescription;
    }

    public void ChangePrice(decimal newPrice)
    {
        Price = ValidatePrice(newPrice);
    }

    public void ChangeStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        
        Stock = quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to increase cannot be negative.");

        Stock += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to decrease cannot be negative.");

        if (Stock - quantity < 0)
            throw new InvalidOperationException("Stock cannot go below zero.");

        Stock -= quantity;
    }

    public void ChangeCategory(Guid categoryId)
    {
        CategoryId = ValidateCategoryId(categoryId);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        return name.Trim();
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        return price;
    }

    private static Guid ValidateCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required.", nameof(categoryId));

        return categoryId;
    }
}
