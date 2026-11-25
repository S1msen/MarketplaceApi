using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Repository;

public interface IListingRepository
{
    Task<List<Listing>> SearchAsync(ListingQueryDto query, CancellationToken ct);
    Task<List<Listing>> GetMineAsync(string userId, CancellationToken ct);
    Task<Listing?> GetAsync(int id, CancellationToken ct);
    Task<List<Listing>> GetAnalyticsDataAsync(int? categoryId, CancellationToken ct);
    void Add(Listing listing);
}
