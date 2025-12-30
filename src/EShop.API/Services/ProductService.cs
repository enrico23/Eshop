using EShop.API.Models;
using EShop.Data;
using EShop.Data.Repositories;
using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Domain.Products;
using EShop.Shared.Domain;
using EShop.Shared.Observability.Correlation;
using EShop.Shared.Representations.Product;
using EShop.Shared.Services;
using EShop.Shared.Services.Eventing;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace EShop.API.Services;

public class ProductService(
    CatalogDbContext context,
    IProductRepository productRepository,
    IMapper mapper,
    IImageStorageService imageStorageService,
    IEventPublisher eventPublisher,
     ICorrelationIdAccessor correlationIdAccessor) 
    : IProductService
{
    private readonly CatalogDbContext _context = context;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IImageStorageService _imageStorageService = imageStorageService;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly ICorrelationIdAccessor _correlationIdAccessor = correlationIdAccessor;

    public async Task<Shared.Representations.PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc",
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var pagedResult = await _productRepository.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, ct);
        var items = pagedResult.Items.Adapt<List<ProductDto>>();
        return new Shared.Representations.PagedResult<ProductDto>(items, page, pageSize, pagedResult.Total);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _productRepository.ExistsAsync(id, ct);

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(id, track: false, ct);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> AddAsync(ProductFormDto productFormDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productFormDto, nameof(productFormDto));

        var productName = Product.ValidateName(productFormDto.Name);

        if (await _productRepository.NameExistsAsync(productName, ct))
            throw new DomainException($"A product with name '{productName}' already exists.");

        var productDescription = new ProductDescription(productFormDto.Description);
        var productPrice = new Money(productFormDto.Price);
        var productQuantity = new StockQuantity(productFormDto.Stock);

        var sku = new Sku(productFormDto.Sku);

        var product = Product.Create(
            productName,
            productDescription,
            productPrice,
            productQuantity,
            productFormDto.CategoryId.GetValueOrDefault(),
            sku);

        if(productFormDto.File is not null)
        {
            var newUrl = await _imageStorageService.UploadImageAsync(productFormDto.File, ct);
            product.AddImage(newUrl);
        }

        await _productRepository.AddAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);

        return product.Adapt<ProductDto>();
    }

    public async Task<bool> UpdateAsync(Guid id, ProductFormDto productFormDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productFormDto, nameof(productFormDto));

        var product = await _productRepository.GetByIdAsync(id, track: true, ct);
        if (product is null)
            return false;

        product.ChangeName(productFormDto.Name);
        product.ChangeDescription(new ProductDescription(productFormDto.Description));
        product.ChangePrice(new Money(productFormDto.Price));
        product.ChangeStock(new StockQuantity(productFormDto.Stock));

        switch (productFormDto.Status)
        {
            case ProductStatus.Active:
                product.Publish();
                break;
            case ProductStatus.Archived:
                product.Archive();
                break;
            case ProductStatus.Draft:
                product.Draft();
                break;
            default:
                product.Deactivate();
                break;
        }

        var newSku = new Sku(productFormDto.Sku);
        if (!product.Sku.Equals(newSku) 
            && await _productRepository.SkuInUseAsync(newSku, product.Id, ct))
                throw new DomainException($"A product with SKU: '{newSku}' already exists.");
        product.ChangeSku(newSku);

        if (!productFormDto.CategoryId.HasValue)
            throw new DomainException("Category is required.");
        product.ChangeCategory((Guid)productFormDto.CategoryId);


        if (productFormDto.File is not null)
        {
            foreach (var image in product.Images)
                await SendDeleteCommandAsync(image.Url, ct);

            var newUrl = await _imageStorageService.UploadImageAsync(productFormDto.File, ct);
            product.AddImage(newUrl);

            var newImages = new List<ProductImageSpec> {
                new ProductImageSpec(Url: newUrl, AltText: productFormDto.Name, IsPrimary: true)
            }.ToImmutableList();

            product.ReplaceImages(newImages!);
        }

        await _productRepository.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _productRepository.GetByIdAsync(id, track: true, ct);
        if (entity is null)
            return false;

        if(entity.Images.Count > 0)
        {
            foreach (var image in entity.Images)
            {
                await SendDeleteCommandAsync(image.Url, ct);
            }
        }

       _productRepository.Remove(entity);
        await _productRepository.SaveChangesAsync(ct);

        return true;
    }

    private async Task SendDeleteCommandAsync(string imageUrl, CancellationToken ct)
    {
        {
            await _eventPublisher.PublishAsync(
                "images-queue",
                new EventEnvelope<DeleteImageRequest>(
                    Type: "image.deleted",
                    Data: new DeleteImageRequest(imageUrl),
                    CorrelationId: _correlationIdAccessor.CorrelationId,
                    Source: "eshop.api"), ct);
        }
    }
}