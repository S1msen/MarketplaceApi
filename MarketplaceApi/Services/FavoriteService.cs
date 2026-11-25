using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;

namespace MarketplaceApi.Services;

public class FavoriteService(IFavoriteRepository favorites, IListingRepository listings, MarketplaceDbContext context)
    : IFavoriteService
{
    public async Task<List<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct) =>
        (await favorites.GetMineAsync(userId, ct)).Select(x => new FavoriteDto(x.Id, x.Listing.ToDto())).ToList();

    public Task<FavoriteDto?> AddAsync(int listingId, string userId, CancellationToken ct) =>
        context.WriteAsync<FavoriteDto?>(async () =>
        {
            var listing = await listings.GetAsync(listingId, ct) ?? throw ServiceError.Missing("Listing");
            if (await favorites.GetAsync(userId, listingId, ct) is not null)
                return null;
            var favorite = new Favorite { UserId = userId, Listing = listing };
            favorites.Add(favorite);
            await context.SaveChangesAsync(ct);
            return new FavoriteDto(favorite.Id, listing.ToDto());
        }, ct);

    public async Task RemoveAsync(int listingId, string userId, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var favorite = await favorites.GetAsync(userId, listingId, ct);
            if (favorite is not null)
                favorites.Remove(favorite);
            return true;
        }, ct);
}
