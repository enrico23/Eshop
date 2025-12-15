namespace EShop.Domain;

public abstract class AuditableEntity : IAuditable
{
    public DateTimeOffset CreatedUtc { get; protected set; }
    public DateTimeOffset? UpdatedUtc { get; protected set; }
}