namespace EShop.Shared.Observability.Correlation;

public static class CorrelationIdValidator
{
    public static bool IsValid(string? value, CorrelationIdOptions options)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length < options.MinLength || value.Length > options.MaxLength)
            return false;

        foreach (var c in value)
        {
            if (!char.IsLetterOrDigit(c) && c is not '-' and not '_' and not '.')
                return false;
        }

        return true;
    }
}
