using EShop.API.Models;
using EShop.Data;
using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Domain.Products;
using EShop.Shared.Domain;
using EShop.Shared.Representations.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq.Dynamic.Core;

namespace EShop.API.Services;


public class ProductService(CatalogDbContext context, IFileStorage fileStorage) 
    : IProductService
{ 
    public async Task<Shared.Representations.PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc",
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<Product> query = context.Products.AsNoTracking();

        if(!string.IsNullOrEmpty(filter))
            query = query.Where("Name.ToLower().Contains(@0)", filter.ToLower());
        

        if (!string.IsNullOrEmpty(orderBy))
        {
            var ordering = $"{orderBy} {orderDirection}";
            query = query.OrderBy(ordering);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .ProjectToType<ProductDto>()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new Shared.Representations.PagedResult<ProductDto>(items, page, pageSize, total);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .AnyAsync(b => b.Id == id, ct);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Products
            .AsNoTracking()
            .Where(b => b.Id == id)
            .ProjectToType<ProductDto>()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProductDto> AddAsync(ProductFormDto productFormDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productFormDto, nameof(productFormDto));

        var productName = Product.ValidateName(productFormDto.Name);

        var alreadyExists = await context.Products
            .AnyAsync(b => b.Name.ToLower() == productName.ToLower(), ct);
        if (alreadyExists)
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

        if (productFormDto.File is not null)
        {
            var result = await fileStorage.SaveImageAsync(productFormDto.File, ct);

            product.AddImage(result);
        }

        await context.Products.AddAsync(product, ct);

        await context.SaveChangesAsync(ct);

        return product.Adapt<ProductDto>();
    }

    public async Task<bool> UpdateAsync(Guid id, ProductFormDto productFormDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productFormDto, nameof(productFormDto));

        var product = await context.Products.FirstOrDefaultAsync(b => b.Id == id, ct);
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
        if (!product.Sku.Equals(newSku))
        {
            var skuInUse = await context.Products
                .AnyAsync(p => p.Sku == newSku && p.Id != product.Id, ct);

            if (skuInUse)
                throw new DomainException($"A product with SKU '{newSku}' already exists.");

            product.ChangeSku(newSku);
        }

        if (!productFormDto.CategoryId.HasValue)
            throw new DomainException("Category is required.");
        product.ChangeCategory((Guid)productFormDto.CategoryId);


        if(productFormDto.Images is not null && productFormDto.Images.Count > 0)
        {
           var newImages = productFormDto.Images.Select(image =>
               new ProductImageSpec( image.Url, image.AltText, image.IsPrimary))
                .ToImmutableList();

            product.ReplaceImages(newImages!);

            if(productFormDto.File is not null)
            {
                await fileStorage.SaveImageAsync(productFormDto.File, ct);
            }
        }
        
        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await context.Products.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null)
            return false;

        if (entity.Images.Count != 0)
        { 
            foreach(var image in entity.Images.ToList())
            {
              await fileStorage.DeleteImageAsync(image.Url, ct);
            }
        }

        context.Products.Remove(entity);
        await context.SaveChangesAsync(ct);

        return true;
    }
}

