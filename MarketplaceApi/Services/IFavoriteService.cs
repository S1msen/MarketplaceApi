using MarketplaceApi.Models;

namespace MarketplaceApi.Services;

public interface IFavoriteService
{
    Task<List<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<FavoriteDto?> AddAsync(int listingId, string userId, CancellationToken ct);
    Task RemoveAsync(int listingId, string userId, CancellationToken ct);
}
