using MarketplaceApi.Data;
using MarketplaceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Repository;

public class OfferRepository(MarketplaceDbContext context) : IOfferRepository
{
    public Task<Offer?> GetAsync(int id, CancellationToken ct) =>
        context.Offers.Include(x => x.Listing).ThenInclude(x => x.Offers)
            .SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Offer>> GetMineAsync(string userId, CancellationToken ct) =>
        context.Offers.AsNoTracking().Include(x => x.Listing).Where(x => x.BuyerId == userId)
            .OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id).ToListAsync(ct);

    public void Add(Offer offer) => context.Offers.Add(offer);
}
