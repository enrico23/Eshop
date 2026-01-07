namespace EShop.Domain;

public interface IEntity<TKey>
{
    TKey Id { get; }
}

