using MarketplaceApi.Data;
using MarketplaceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Repository;

public class FavoriteRepository(MarketplaceDbContext context) : IFavoriteRepository
{
    public Task<List<Favorite>> GetMineAsync(string userId, CancellationToken ct) =>
        context.Favorites.AsNoTracking().Where(x => x.UserId == userId)
            .Include(x => x.Listing).ThenInclude(x => x.Category)
            .Include(x => x.Listing).ThenInclude(x => x.Seller)
            .Include(x => x.Listing).ThenInclude(x => x.Offers)
            .AsSplitQuery().OrderByDescending(x => x.Id).ToListAsync(ct);

    public Task<Favorite?> GetAsync(string userId, int listingId, CancellationToken ct) =>
        context.Favorites.SingleOrDefaultAsync(x => x.UserId == userId && x.ListingId == listingId, ct);

    public void Add(Favorite favorite) => context.Favorites.Add(favorite);
    public void Remove(Favorite favorite) => context.Favorites.Remove(favorite);
}
