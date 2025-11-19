namespace EShop.API.Representations.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public int CategoryId { get; set; } 
    public string CategoryName { get; set; } = null!;

    public int BrandId { get; set; }
    public string BrandName { get; set; } = null!;
}
