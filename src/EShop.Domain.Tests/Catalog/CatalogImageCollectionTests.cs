using EShop.Domain.Exceptions;

namespace EShop.Domain.Tests.Catalog;

public sealed class CatalogImageCollectionTests
{
    [Fact]
    public void Add_FirstImage_SetsPrimaryAndSortOrderZero()
    {
        var sut = CreateSut();

        var image = sut.Add("https://cdn/a.png", " A ");

        Assert.Single(sut.Items);
        Assert.True(image.IsPrimary);
        Assert.Equal(0, image.SortOrder);
        Assert.Equal("A", image.AltText);
    }

    [Fact]
    public void Add_SecondImage_IncrementsSortOrderAndIsNotPrimary()
    {
        var sut = CreateSut();
        sut.Add("https://cdn/a.png");

        var image = sut.Add("https://cdn/b.png");

        Assert.Equal(2, sut.Items.Count);
        Assert.False(image.IsPrimary);
        Assert.Equal(1, image.SortOrder);
    }

    [Fact]
    public void Add_DuplicateUrl_ThrowsDomainException()
    {
        var sut = CreateSut();
        sut.Add("https://cdn/a.png");

        var ex = Assert.Throws<DomainException>(() => sut.Add("https://cdn/a.png"));

        Assert.Equal("This image is already associated with the product.", ex.Message);
    }

    [Fact]
    public void Remove_ImageNotFound_ThrowsDomainException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<DomainException>(() => sut.Remove(Guid.NewGuid()));

        Assert.Equal("Image not found.", ex.Message);
    }

    [Fact]
    public void Remove_RemovingPrimary_PromotesFirstBySort()
    {
        var sut = CreateSut();
        var first = sut.Add("https://cdn/a.png");
        var second = sut.Add("https://cdn/b.png");

        sut.Remove(first.Id);

        Assert.Single(sut.Items);
        Assert.True(second.IsPrimary);
    }

    [Fact]
    public void SetPrimary_ImageNotFound_ThrowsDomainException()
    {
        var sut = CreateSut();
        sut.Add("https://cdn/a.png");

        var ex = Assert.Throws<DomainException>(() => sut.SetPrimary(Guid.NewGuid()));

        Assert.Equal("Image not found.", ex.Message);
    }

    [Fact]
    public void SetPrimary_UpdatesSinglePrimaryInvariant()
    {
        var sut = CreateSut();
        var first = sut.Add("https://cdn/a.png");
        var second = sut.Add("https://cdn/b.png");

        sut.SetPrimary(second.Id);

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
        Assert.Equal(1, sut.Items.Count(i => i.IsPrimary));
    }

    [Fact]
    public void Reorder_InvalidCount_ThrowsDomainException()
    {
        var sut = CreateSut();
        var first = sut.Add("https://cdn/a.png");
        sut.Add("https://cdn/b.png");

        var ex = Assert.Throws<DomainException>(() => sut.Reorder([first.Id]));

        Assert.Equal("Invalid image ordering.", ex.Message);
    }

    [Fact]
    public void Reorder_UnknownId_ThrowsDomainException()
    {
        var sut = CreateSut();
        var first = sut.Add("https://cdn/a.png");
        sut.Add("https://cdn/b.png");

        var ex = Assert.Throws<DomainException>(() => sut.Reorder([first.Id, Guid.NewGuid()]));

        Assert.Equal("Invalid image ordering.", ex.Message);
    }

    [Fact]
    public void Reorder_ValidOrder_UpdatesSortOrder()
    {
        var sut = CreateSut();
        var first = sut.Add("https://cdn/a.png");
        var second = sut.Add("https://cdn/b.png");
        var third = sut.Add("https://cdn/c.png");

        sut.Reorder([third.Id, first.Id, second.Id]);

        Assert.Equal(1, first.SortOrder);
        Assert.Equal(2, second.SortOrder);
        Assert.Equal(0, third.SortOrder);
    }

    [Fact]
    public void Replace_Null_ThrowsArgumentNullException()
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentNullException>(() => sut.Replace(null!));
    }

    [Fact]
    public void Replace_Empty_ClearsCollection()
    {
        var sut = CreateSut();
        sut.Add("https://cdn/a.png");

        sut.Replace([]);

        Assert.Empty(sut.Items);
    }

    [Fact]
    public void Replace_MoreThanOnePrimary_ThrowsDomainException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<DomainException>(() => sut.Replace(
        [
            new CatalogImageSpec("https://cdn/a.png", "A", true),
            new CatalogImageSpec("https://cdn/b.png", "B", true)
        ]));

        Assert.Equal("Only one primary image is allowed.", ex.Message);
    }

    [Fact]
    public void Replace_WithRequestedPrimary_SetsRequestedImageAsPrimary()
    {
        var sut = CreateSut();

        sut.Replace(
        [
            new CatalogImageSpec("https://cdn/a.png", "A", false),
            new CatalogImageSpec("https://cdn/b.png", "B", true)
        ]);

        var first = sut.Items.Single(i => i.Url == "https://cdn/a.png");
        var second = sut.Items.Single(i => i.Url == "https://cdn/b.png");

        Assert.False(first.IsPrimary);
        Assert.True(second.IsPrimary);
        Assert.Equal(1, sut.Items.Count(i => i.IsPrimary));
    }

    [Fact]
    public void Replace_WithDuplicateUrls_ThrowsDomainException()
    {
        var sut = CreateSut();

        var ex = Assert.Throws<DomainException>(() => sut.Replace(
        [
            new CatalogImageSpec("https://cdn/a.png", "A", false),
            new CatalogImageSpec("https://cdn/a.png", "A2", false)
        ]));

        Assert.Equal("This image is already associated with the product.", ex.Message);
    }

    private static CatalogImageCollection CreateSut(string ownerName = "product")
        => new(ownerName);
}
