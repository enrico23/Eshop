namespace EShop.Shared.Representations;

public class BaseEntityDto
{
    public DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset? UpdatedUtc { get; set; }
}