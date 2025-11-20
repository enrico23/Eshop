using EShop.API.Exceptions;
using EShop.API.Representations.Brand;
using EShop.Data;
using EShop.Domain;
using EShop.Shared.Representations;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace EShop.API.Services;

public class BrandService : IBrandService
{
    private readonly CatalogDbContext _context;

    private readonly IMapper _mapper;

    public BrandService(CatalogDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<BrandDto>> GetPagedAsync(int page, int pageSize, 
        string orderBy = "Name", string orderDirection = "asc", CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<Brand> query = _context.Brands.AsNoTracking();

        if (!string.IsNullOrEmpty(orderBy))
        {
            var ordering = $"{orderBy} {orderDirection}";
            query = query.OrderBy(ordering);
        }

        var total = await query.LongCountAsync(ct);

        var items = await query
            .ProjectToType<BrandDto>()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedResult<BrandDto>(items, page, pageSize, total);
    }

    public async Task<bool> BrandExistsAsync(int brandId, CancellationToken ct = default)
    {
        return await _context.Brands
            .AsNoTracking()
            .AnyAsync(b => b.Id == brandId, ct);
    }

    public async Task<BrandDto?> GetByIdAsync(int brandId, CancellationToken ct = default)
    {
       return await _context.Brands
            .AsNoTracking()
            .Where(b => b.Id == brandId)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateBrandDto updateBrandDto, CancellationToken ct)
    {
        if (updateBrandDto is null) 
            throw new ArgumentNullException(nameof(updateBrandDto));

        var entity = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null)
            return false;

        updateBrandDto.Adapt(entity);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BrandDto> AddAsync(CreateBrandDto createBrandDto, CancellationToken ct)
    {
        if (createBrandDto is null) 
            throw new ArgumentNullException(nameof(createBrandDto));

        var alreadyExists = await _context.Brands.AnyAsync(b => b.Name.ToLower() == createBrandDto.Name.ToLower(), ct);
        if (alreadyExists)
            throw new DomainException($"A brand with name '{createBrandDto.Name}' already exists.");

        var name = createBrandDto.Name?.Trim();
        var brand = _mapper.From(createBrandDto).AdaptToType<Brand>();

        await _context.Brands.AddAsync(brand, ct);
        await _context.SaveChangesAsync(ct);

        return brand.Adapt<BrandDto>();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null)
            return false;

        _context.Brands.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return true;
    }
}
