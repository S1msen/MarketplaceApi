using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Repository;

public interface IOfferRepository
{
    Task<Offer?> GetAsync(int id, CancellationToken ct);
    Task<List<Offer>> GetMineAsync(string userId, CancellationToken ct);
    void Add(Offer offer);
}
