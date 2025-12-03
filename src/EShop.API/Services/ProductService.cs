using EShop.API.Exceptions;
using EShop.Data;
using EShop.Domain;
using EShop.Shared.Representations;
using EShop.Shared.Representations.Product;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EShop.API.Services;

public class ProductService : IProductService
{
    private readonly CatalogDbContext _context;

    private readonly IMapper _mapper;

    public ProductService(CatalogDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
 
    public async Task<Shared.Representations.PagedResult<ProductDto>> GetPagedAsync(
        int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc",
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<Product> query = _context.Products.AsNoTracking();

        if(!string.IsNullOrEmpty(filter))
        {
            query = query.Where("Name.ToLower().Contains(@0) OR Description.ToLower().Contains(@0)", filter.ToLower());
        }

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
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(b => b.Id == id, ct);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(b => b.Id == id)
            .ProjectToType<ProductDto>()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProductDto> AddAsync(CreateProductDto createProductDto, CancellationToken ct)
    {
        if (createProductDto is null)
            throw new ArgumentNullException(nameof(createProductDto));

        var alreadyExists = await _context.Products.AnyAsync(b => b.Name.ToLower() == createProductDto.Name.ToLower(), ct);
        if (alreadyExists)
            throw new DomainException($"A brand with name '{createProductDto.Name}' already exists.");

        var name = createProductDto.Name?.Trim();
        var product = _mapper.From(createProductDto).AdaptToType<Product>();

        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);

        return product.Adapt<ProductDto>();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProductDto updateProductDto, CancellationToken ct)
    {
        if (updateProductDto is null)
            throw new ArgumentNullException(nameof(updateProductDto));

        var entity = await _context.Products.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null)
            return false;

        updateProductDto.Adapt(entity);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Products.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null)
            return false;

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}