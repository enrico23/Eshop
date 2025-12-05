namespace EShop.Domain.Products;

public sealed class ProductDescription
{
    public string Value { get; private set; }

    public ProductDescription()
    {
        Value = string.Empty;
    }

    public ProductDescription(string value)
    {
        value ??= string.Empty;
        value = value.Trim();

        if (value.Length > 2000)
            throw new ArgumentException("Product description is limited to 2000 characters");

        Value = value;
    }

    public override string ToString() => Value;
}
