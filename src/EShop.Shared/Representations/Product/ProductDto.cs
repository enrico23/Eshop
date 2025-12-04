namespace EShop.Shared.Representations.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}
