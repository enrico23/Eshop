namespace EShop.Domain;

public interface IAuditable
{
    DateTimeOffset CreatedUtc { get; }
    DateTimeOffset? UpdatedUtc { get; }
}
