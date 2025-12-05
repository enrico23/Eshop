namespace EShop.Domain.Catalog;

public sealed class Category
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public Category()
    {
    }

    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = ValidateName(name);
    }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = ValidateName(name);
    }

    public void ChangeName(string newName)
    {
        Name = ValidateName(newName);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        return name.Trim();
    }
}
