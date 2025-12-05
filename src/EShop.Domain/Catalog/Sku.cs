namespace EShop.Domain.Catalog;

public sealed class Sku : IEquatable<Sku>
{
    public string Value { get; }

    // For EF Core
    private Sku()
    {
        Value = string.Empty;
    }

    public Sku(string value)
    {
        // ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
        if(string.IsNullOrEmpty(value))
            value = string.Empty;

        value = value.Trim();

        if (value.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters.", nameof(value));

        Value = value.ToUpperInvariant();
    }

    public override string ToString() => Value;

    // Equality based on Value
    public bool Equals(Sku? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Value == other.Value;
    }

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is Sku other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
}
