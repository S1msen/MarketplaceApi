using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;

namespace MarketplaceApi.Services;

public class CategoryService(ICategoryRepository categories, MarketplaceDbContext context) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct) =>
        (await categories.GetAllAsync(ct)).Select(x => x.ToDto()).ToList();

    public async Task<CategoryDto> GetAsync(int id, CancellationToken ct) =>
        (await categories.GetAsync(id, ct) ?? throw ServiceError.Missing("Category")).ToDto();

    public async Task<CategoryDto> CreateAsync(CategoryWriteDto dto, CancellationToken ct)
    {
        var category = new Category { Name = dto.Name.Trim() };
        categories.Add(category);
        await context.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task UpdateAsync(int id, CategoryWriteDto dto, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var category = await categories.GetAsync(id, ct) ?? throw ServiceError.Missing("Category");
            category.Name = dto.Name.Trim();
            return true;
        }, ct);

    public async Task DeleteAsync(int id, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var category = await categories.GetAsync(id, ct) ?? throw ServiceError.Missing("Category");
            if (await categories.HasListingsAsync(id, ct))
                throw ServiceError.Conflict("A category containing listings cannot be deleted.");
            categories.Remove(category);
            return true;
        }, ct);
}
