using EShop.API.Representations;
using EShop.Data;
using EShop.Domain;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Services;

public class BrandService(CatalogDbContext context, IMapper mapper) : IBrandService
{
    public async Task<PaginatedResult<BrandDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = context.Brands.AsNoTracking().OrderBy(b => b.Name);

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
        return await context.Brands.AsNoTracking().AnyAsync(b => b.Id == brandId, ct);
    }

    public async Task<BrandDto?> GetByIdAsync(int brandId, CancellationToken ct = default)
    {
       return await context.Brands
            .AsNoTracking()
            .Where(b => b.Id == brandId)
            .ProjectToType<BrandDto>()
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateBrandDto updateBrandDto, CancellationToken ct)
    {
        if (updateBrandDto is null) 
            throw new ArgumentNullException(nameof(updateBrandDto));

        var entity = await context.Brands.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null)
            return false;

        updateBrandDto.Adapt(entity);

        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BrandDto> AddAsync(CreateBrandDto createBrandDto, CancellationToken ct)
    {
        if (createBrandDto is null) 
            throw new ArgumentNullException(nameof(createBrandDto));

        var brand = mapper.From(createBrandDto).AdaptToType<Brand>();

        var alreadyExists = await context.Brands.AnyAsync(b => b.Name == createBrandDto.Name, ct);
        if (alreadyExists)
            throw new DomainException($"A brand with name '{createBrandDto.Name}' already exists.");

        await context.Brands.AddAsync(brand, ct);
        await context.SaveChangesAsync(ct);

        return brand.Adapt<BrandDto>();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await context.Brands
                .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (entity is null)
            return false;

        context.Brands.Remove(entity);
        await context.SaveChangesAsync(ct);

        return true;
    }
}
