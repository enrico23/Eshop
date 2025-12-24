using EShop.Domain.Validation;

namespace EShop.Domain.Tests.Validation;

public sealed class NameValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrWhitespace_ThrowsArgumentNullException(string? value)
    {
      Assert.Throws<ArgumentException>(() => NameValidation.Validate(value!, 200, "Product name"));
    }

    [Fact]
    public void Validate_TrimsValue()
    {
        var result = NameValidation.Validate("  Shoes  ", 200, "Product name");

        Assert.Equal("Shoes", result);
    }

    [Fact]
    public void Validate_WhenLengthEqualsMax_Allows()
    {
        var value = new string('a', 200);

        var result = NameValidation.Validate(value, 200, "Product name");

        Assert.Equal(value, result);
    }

    [Fact]
    public void Validate_WhenLengthExceedsMax_ThrowsArgumentException()
    {
        var value = new string('a', 201);

        Assert.Throws<ArgumentException>(() => NameValidation.Validate(value, 200, "Product name"));
    }

    [Fact]
    public void Validate_UsesTrimmedLengthForMax()
    {
        var result = NameValidation.Validate("  aa  ", 2, "Product name");

        Assert.Equal("aa", result);
    }
}
