namespace EShop.Domain;

public sealed class StockQuantity
{
    public int Value { get; }

    private StockQuantity()
    {
        Value = 0;
    }

    public StockQuantity(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stock cannot be negative.");

        Value = value;
    }

    public StockQuantity Increase(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Increase amount must be positive.");

        return new StockQuantity(Value + amount);
    }

    public StockQuantity Decrease(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Decrease amount must be positive.");

        if (Value - amount < 0)
            throw new InvalidOperationException("Stock cannot go below zero.");

        return new StockQuantity(Value - amount);
    }

    public override string ToString() => Value.ToString();
}

