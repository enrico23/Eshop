using System.Globalization;

namespace EShop.Domain.Validation;

public static class NameValidation
{
    public static string Validate(string value, int maxLength, string displayName, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(value);

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{displayName} is limited to {maxLength} characters.", nameof(value));

        return trimmed;
    }
}