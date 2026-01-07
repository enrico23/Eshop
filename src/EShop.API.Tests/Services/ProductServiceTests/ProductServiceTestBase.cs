using EShop.Data;
using EShop.Shared.Exceptions;
using System.Collections.Immutable;

namespace EShop.API.Tests.Services.ProductServiceTests;

public abstract class ProductServiceTestBase
{
    internal IExtendedProductRepository _repo = Substitute.For<IExtendedProductRepository>();
    internal IRepository<Product, Guid, CatalogDbContext> _repo2 = Substitute.For<IRepository<Product, Guid, CatalogDbContext>>();
    internal IMapper _mapper = Substitute.For<IMapper>();
    internal IImageStorageService _imageStorage = Substitute.For<IImageStorageService>();
    internal IEventPublisher _publisher = Substitute.For<IEventPublisher>();
    internal ICorrelationIdAccessor _corr = Substitute.For<ICorrelationIdAccessor>();
    internal IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    internal ISqlServerExceptionsHelper _sqlServerExceptionsHelper = Substitute.For<ISqlServerExceptionsHelper>();

    internal ProductService CreateSut() => new ProductService(_repo, _repo2, _mapper, _imageStorage, _publisher, _corr, _unitOfWork, _sqlServerExceptionsHelper);

    internal static Product CreateProductWithImages(Guid id, string sku, string[] imageUrls)
    {
        var p = Product.Create(
            Product.ValidateName("P"),
            new ProductDescription("D"),
            new Money(1m),
            new StockQuantity(1),
            categoryId: Guid.NewGuid(),
            sku: new Sku(sku));

        if (imageUrls.Length > 0)
        {
            var specs = imageUrls
                .Select((u, idx) => new ProductImageSpec(Url: u, AltText: "alt", IsPrimary: idx == 0))
                .ToImmutableList();

            p.ReplaceImages(specs);
        }

        return p;
    }

    internal static ProductFormDto ValidForm(ProductStatus status, bool withFile)
        => new ProductFormDto
        {
            Name = "Name",
            Description = "Description",
            Price = 12.34m,
            Stock = 10,
            Sku = "SKU-1",
            CategoryId = Guid.NewGuid(),
            Status = status,
            File = withFile ? FormFileHelpers.CreateFakeFormFile() : null
        };
}
