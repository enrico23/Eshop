using EShop.Shared.Domain;

namespace EShop.Shared.Representations.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public ProductStatus Status {get; set;}

    public Guid CategoryId { get; set; } 
    public string CategoryName { get; set; } = null!;
}
