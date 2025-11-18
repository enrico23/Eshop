namespace EShop.API.Services;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}