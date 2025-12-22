namespace EShop.Shared.Tests.Observability.Correlation;

public sealed class CorrelationIdValidatorTests
{
    private static CorrelationIdOptions Options(int min = 8, int max = 64)
        => new() { MinLength = min, MaxLength = max };

    [Theory]
    [InlineData("abcdef12")]
    [InlineData("ABCDEF12")]
    [InlineData("abcDEF12-_.")]
    [InlineData("a1b2c3d4e5f6g7h8")]
    public void IsValid_AllowsExpectedValues(string value)
    {
        Assert.True(CorrelationIdValidator.IsValid(value, Options()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short")]
    [InlineData("has space")]
    [InlineData("bad!char")]
    public void IsValid_RejectsInvalidValues(string? value)
    {
        Assert.False(CorrelationIdValidator.IsValid(value, Options()));
    }

    [Fact]
    public void IsValid_RejectsTooLongValues()
    {
        var value = new string('a', 65);
        Assert.False(CorrelationIdValidator.IsValid(value, Options()));
    }
}
