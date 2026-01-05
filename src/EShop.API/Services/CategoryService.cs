using EShop.Data;
using EShop.Data.Repositories;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Shared.Representations.Category;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Services;

public class CategoryService(
    ICategoryRepository categoryRepository, 
    IMapper mapper,
    IUnitOfWork unitOfWork) 
    : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Shared.Representations.PagedResult<CategoryDto>> GetPagedAsync(
        int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", 
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var pagedResult = await _categoryRepository.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, ct);
        var items = _mapper.Map<List<CategoryDto>>(pagedResult.Items);
        return new Shared.Representations.PagedResult<CategoryDto>(items, page, pageSize, pagedResult.Total);
    }

    public async Task<ICollection<CategoryDto>> GetAllAsync(CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
         await _categoryRepository.ExistsAsync(id, ct);
    
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, false, ct);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
     }

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(updateCategoryDto, nameof(updateCategoryDto));

        var entity = await _categoryRepository.GetByIdAsync(id, true, ct);
        if (entity is null)
            return false;

        var categoryName = updateCategoryDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        entity.ChangeName(updateCategoryDto.Name!);

        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CategoryDto> AddAsync(CreateCategoryDto createCategoryDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(createCategoryDto, nameof(createCategoryDto));

        var categoryName = createCategoryDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        var alreadyExists = await _categoryRepository.NameExistsAsync(categoryName, ct);
        if (alreadyExists)
            throw new DomainException($"A category with name '{categoryName}' already exists.");

        var category = new Category(categoryName);

        await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return category.Adapt<CategoryDto>();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _categoryRepository.GetByIdAsync(id, true, ct);

        if (entity is null) return false;

        var hasProducts = await _categoryRepository.HasAnyProductsAsync(id, ct);
        if (hasProducts)
            throw new DomainException("Category cannot be deleted because it contains products.");

        _categoryRepository.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
