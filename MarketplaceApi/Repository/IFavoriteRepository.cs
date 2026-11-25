using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Repository;

public interface IFavoriteRepository
{
    Task<List<Favorite>> GetMineAsync(string userId, CancellationToken ct);
    Task<Favorite?> GetAsync(string userId, int listingId, CancellationToken ct);
    void Add(Favorite favorite);
    void Remove(Favorite favorite);
}
