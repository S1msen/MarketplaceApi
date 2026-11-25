using MarketplaceApi.Data;
using MarketplaceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Repository;

public class CategoryRepository(MarketplaceDbContext context) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        context.Categories.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);

    public Task<Category?> GetAsync(int id, CancellationToken ct) =>
        context.Categories.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> HasListingsAsync(int id, CancellationToken ct) =>
        context.Listings.AnyAsync(x => x.CategoryId == id, ct);

    public void Add(Category category) => context.Categories.Add(category);
    public void Remove(Category category) => context.Categories.Remove(category);
}
