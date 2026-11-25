using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Repository;

public class ListingRepository(MarketplaceDbContext context) : IListingRepository
{
    private IQueryable<Listing> Details => context.Listings
        .Include(x => x.Category).Include(x => x.Seller).Include(x => x.Offers)
        .Include(x => x.PriceChanges).AsSplitQuery();

    public async Task<List<Listing>> SearchAsync(ListingQueryDto query, CancellationToken ct)
    {
        var listings = Details.AsNoTracking().Where(x => x.Status == ListingStatus.Active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            listings = listings.Where(x => x.Title.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }
        if (query.CategoryId is { } categoryId)
            listings = listings.Where(x => x.CategoryId == categoryId);
        if (query.Condition is { } condition)
            listings = listings.Where(x => x.Condition == condition);
        if (query.MinPrice is { } min)
            listings = listings.Where(x => x.Price >= min);
        if (query.MaxPrice is { } max)
            listings = listings.Where(x => x.Price <= max);

        return await (query.Sort switch
        {
            "price_asc" => listings.OrderBy(x => x.Price).ThenByDescending(x => x.Id),
            "price_desc" => listings.OrderByDescending(x => x.Price).ThenByDescending(x => x.Id),
            _ => listings.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id)
        }).ToListAsync(ct);
    }

    public Task<List<Listing>> GetMineAsync(string userId, CancellationToken ct) =>
        Details.AsNoTracking().Where(x => x.SellerId == userId)
            .OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id).ToListAsync(ct);

    public Task<Listing?> GetAsync(int id, CancellationToken ct) =>
        Details.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Listing>> GetAnalyticsDataAsync(int? categoryId, CancellationToken ct) =>
        context.Listings.AsNoTracking().Include(x => x.PriceChanges)
            .Where(x => categoryId == null || x.CategoryId == categoryId).ToListAsync(ct);

    public void Add(Listing listing) => context.Listings.Add(listing);
}
