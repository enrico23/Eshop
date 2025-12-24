using Microsoft.AspNetCore.Http;

namespace EShop.Shared.Exceptions;

public sealed class BadRequestException : IOException
{
    public BadRequestException(string message, int statusCode) 
        : base(message)
    {
        StatusCode = statusCode;
    }

    public BadRequestException(string message)
       : base(message)
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }

    public BadRequestException(string message, int statusCode, Exception innerException)
       : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public BadRequestException(string message, Exception innerException)
       : base(message, innerException)
    {
        StatusCode = StatusCodes.Status400BadRequest;
    }


    public int StatusCode { get; }
}
