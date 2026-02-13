using EShop.API.Models;
using EShop.Data;
using EShop.Data.Repositories;
using EShop.Domain;
using EShop.Domain.Catalog;
using EShop.Domain.Exceptions;
using EShop.Shared.Exceptions;
using EShop.Shared.Representations;
using EShop.Shared.Representations.Category;
using EShop.Shared.Services;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace EShop.API.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetPagedAsync(int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", string? filter = null, CancellationToken ct = default);

    Task<ICollection<CategoryDto>> GetAllAsync(CancellationToken ct);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> UpdateAsync(Guid id, CategoryFormModel updateCategoryDto, CancellationToken ct);

    Task<CategoryDto> AddAsync(CategoryFormModel createCategoryDto, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class CategoryService(
    IExtendedCategoryRepository categoryRepository, 
    IRepository<Category, Guid, CatalogDbContext> categoryRepo, 
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ISqlServerExceptionsHelper sqlServerExceptionsHelper,
    IImageStorageService imageStorageService) 
    : ICategoryService
{
    private readonly IExtendedCategoryRepository _categoryRepository = categoryRepository;
    private readonly IRepository<Category, Guid, CatalogDbContext> _categoryRepo = categoryRepo;
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISqlServerExceptionsHelper _sqlServerExceptionsHelper = sqlServerExceptionsHelper;
    private readonly IImageStorageService _imageStorageService = imageStorageService;

    public async Task<Shared.Representations.PagedResult<CategoryDto>> GetPagedAsync(
        int page, int pageSize,
        string orderBy = "Name", string orderDirection = "asc", 
        string? filter = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var pagedResult = await _categoryRepo.GetPagedAsync(page, pageSize, orderBy, orderDirection, filter, ct);
        var items = _mapper.Map<List<CategoryDto>>(pagedResult.Items);
        return new Shared.Representations.PagedResult<CategoryDto>(items, page, pageSize, pagedResult.Total);
    }

    public async Task<ICollection<CategoryDto>> GetAllAsync(CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
         await _categoryRepo.ExistsAsync(id, ct);
    
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(id, false, ct);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
     }

    public async Task<bool> UpdateAsync(Guid id, CategoryFormModel categoryFormModel, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(categoryFormModel, nameof(categoryFormModel));

        var entity = await _categoryRepo.GetByIdAsync(id, true, ct);
        if (entity is null)
            return false;

        var categoryName = categoryFormModel.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        entity.ChangeName(categoryFormModel.Name!);

        if (categoryFormModel.File is not null)
        {
            // todo: SendDeleteCommand for image

            //var newUrl = await _imageStorageService.UploadImageAsync(
            //   ImageStoragePurpose.Categories, 
            //   categoryFormModel.File, ct);

            //entity.AddImage(newUrl);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CategoryDto> AddAsync(CategoryFormModel categoryFormDto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(categoryFormDto, nameof(categoryFormDto));

        var categoryName = categoryFormDto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainException("Category name is required.");

        var category = Category.Create(categoryName);

        if (categoryFormDto.File is not null)
        {
            var newUrl = await _imageStorageService.UploadImageAsync(
               ImageStoragePurpose.Categories,
               categoryFormDto.File, ct);

            category.AddImage(newUrl);
        }

        try
        {
            await _categoryRepo.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return category.Adapt<CategoryDto>();
        }
        catch (DbUpdateException ex) when (_sqlServerExceptionsHelper.IsUniqueViolation(ex))
        {
            throw new DomainException($"A category with name '{categoryName}' already exists.");
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _categoryRepo.GetByIdAsync(id, true, ct);

        if (entity is null) return false;

        var hasProducts = await _categoryRepository.HasAnyProductsAsync(id, ct);
        if (hasProducts)
            throw new DomainException("Category cannot be deleted because it contains products.");

        _categoryRepo.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return true;
    }
}
