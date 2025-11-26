using System.Net;

namespace EShop.Web.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseContent { get; }

    public ApiException(HttpStatusCode statusCode, string? content = null)
        : base($"API returned {(int)statusCode}: {content}")
    {
        StatusCode = statusCode;
        ResponseContent = content;
    }
}
