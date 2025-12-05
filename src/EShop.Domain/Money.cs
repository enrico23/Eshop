namespace EShop.Domain;

public sealed class Money
{
    public decimal Amount { get;}

    public Money() => Amount = 0m;

    public Money(decimal amount)
    {
        if(amount < 0)
            throw new ArgumentOutOfRangeException("Amount cannot be a negative number.");
         
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public Money Add(Money other) => new(Amount + other.Amount);

    public Money Subtract(Money other)
    {
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Resulting money amount cannot be negative.");

        return new Money(result);
    }

    public override string ToString() => Amount.ToString("0.00");
}
