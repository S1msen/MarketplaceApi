using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Repository;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetAsync(int id, CancellationToken ct);
    Task<bool> HasListingsAsync(int id, CancellationToken ct);
    void Add(Category category);
    void Remove(Category category);
}
