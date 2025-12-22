namespace EShop.Shared.Tests.Observability.Correlation;

public sealed class CorrelationIdGeneratorTests
{
    [Fact]
    public void CreateId_Returns32CharLowerHex()
    {
        var id = CorrelationIdGenerator.CreateId();

        Assert.Equal(32, id.Length);

        foreach (var c in id)
        {
            var isHexLower = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');
            Assert.True(isHexLower);
        }
    }
}
