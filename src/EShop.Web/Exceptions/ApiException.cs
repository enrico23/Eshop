using System.Net;

namespace EShop.Web.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string? ResponseContent { get; }

    public string? FriendlyMessage { get; }

    public ApiException(HttpStatusCode statusCode, string? content = null)
        : base($"API returned {(int)statusCode}: {content}")
    {
        StatusCode = statusCode;
        ResponseContent = content;
        FriendlyMessage = GetFriendlyMessage(statusCode);
    }

    private string? GetFriendlyMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.InternalServerError => "Server error",
            HttpStatusCode.NotFound => "Resource not found",
            HttpStatusCode.BadRequest => "Invalid request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.Conflict => "Conflict",
            _ => "An unexpected error occurred.Please try again."
        };
    }
}
