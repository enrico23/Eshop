namespace EShop.Domain;

public class Product
{
    private decimal _price;
    private int _stock;
    private string _name = null!;
    private string _sku = null!;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Product name cannot be empty.", nameof(value))
            : value.Trim();
    }

    public string? Description { get; set; }

    public string Sku
    {
        get => _sku;
        set => _sku = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("SKU cannot be empty.", nameof(value))
            : value.Trim();
    }

    public decimal Price
    {
        get => _price;
        set => _price = value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(value), "Price must be greater than zero.")
            : value;
    }

    public int Stock
    {
        get => _stock;
        set => _stock = value < 0
            ? throw new ArgumentOutOfRangeException(nameof(value), "Stock cannot be negative.")
            : value;
    }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
