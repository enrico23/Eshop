using EShop.Domain.Validation;

namespace EShop.Domain.Catalog;

public sealed class Category : AuditableEntity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = default!;

    public ICollection<Product> Products { get; private set; } = [];

    public Category() {}

    public Category(string name)
    {
        Id = Guid.NewGuid();
        Name = NameValidation.Validate(name, 200, "Category name");
    }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = NameValidation.Validate(name, 200, "Category name");
    }

    public void ChangeName(string newName) => Name = NameValidation.Validate(newName, 200, "Category name");
}
