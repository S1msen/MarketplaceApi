using MarketplaceApi.Models;

namespace MarketplaceApi.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto> GetAsync(int id, CancellationToken ct);
    Task<CategoryDto> CreateAsync(CategoryWriteDto dto, CancellationToken ct);
    Task UpdateAsync(int id, CategoryWriteDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
