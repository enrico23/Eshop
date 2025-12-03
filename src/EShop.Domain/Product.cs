namespace EShop.Domain;

public class Product
{

    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Product()
    {
    }
}
