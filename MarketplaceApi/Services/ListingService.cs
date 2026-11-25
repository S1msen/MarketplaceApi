using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;

namespace MarketplaceApi.Services;

public class ListingService(IListingRepository listings, ICategoryRepository categories,
    IAnalyticsService analytics, MarketplaceDbContext context) : IListingService
{
    public async Task<List<ListingDto>> SearchAsync(ListingQueryDto query, CancellationToken ct) =>
        (await listings.SearchAsync(query, ct)).Select(x => x.ToDto()).ToList();

    public async Task<ListingDto> GetAsync(int id, CancellationToken ct)
    {
        var listing = await listings.GetAsync(id, ct) ?? throw ServiceError.Missing("Listing");
        return listing.ToDto() with { PriceAssessment = await analytics.AssessAsync(listing, ct) };
    }

    public async Task<List<ListingDto>> GetMineAsync(string userId, CancellationToken ct) =>
        (await listings.GetMineAsync(userId, ct)).Select(x => x.ToDto()).ToList();

    public async Task<ListingDto> CreateAsync(ListingWriteDto dto, string userId, CancellationToken ct)
    {
        var id = await context.WriteAsync(async () =>
        {
            var category = await categories.GetAsync(dto.CategoryId, ct) ?? throw ServiceError.Missing("Category");
            var listing = new Listing
            {
                Title = dto.Title.Trim(), Description = dto.Description, Price = dto.Price,
                Condition = dto.Condition!.Value, ImageUrl = NormalizeUrl(dto.ImageUrl),
                Category = category, SellerId = userId,
                Status = ListingStatus.Active, CreatedDate = DateTime.UtcNow
            };
            listings.Add(listing);
            await context.SaveChangesAsync(ct);
            return listing.Id;
        }, ct);
        return await GetAsync(id, ct);
    }

    public async Task UpdateAsync(int id, ListingWriteDto dto, string userId, bool isAdmin, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var listing = await listings.GetAsync(id, ct) ?? throw ServiceError.Missing("Listing");
            Authorize(listing, userId, isAdmin);
            if (listing.Status != ListingStatus.Active)
                throw ServiceError.Conflict("Only active listings can be edited.");
            var category = await categories.GetAsync(dto.CategoryId, ct) ?? throw ServiceError.Missing("Category");
            if (listing.Price != dto.Price)
                listing.PriceChanges.Add(new PriceChange
                {
                    OldPrice = listing.Price, NewPrice = dto.Price, ChangedDate = DateTime.UtcNow
                });
            listing.Title = dto.Title.Trim();
            listing.Description = dto.Description;
            listing.Price = dto.Price;
            listing.Condition = dto.Condition!.Value;
            listing.ImageUrl = NormalizeUrl(dto.ImageUrl);
            listing.Category = category;
            return true;
        }, ct);

    public async Task RemoveAsync(int id, string userId, bool isAdmin, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var listing = await listings.GetAsync(id, ct) ?? throw ServiceError.Missing("Listing");
            Authorize(listing, userId, isAdmin);
            if (listing.Status == ListingStatus.Sold)
                throw ServiceError.Conflict("Sold listings cannot be removed.");
            listing.Status = ListingStatus.Removed;
            foreach (var offer in listing.Offers.Where(x => x.Status == OfferStatus.Pending))
                offer.Status = OfferStatus.Rejected;
            return true;
        }, ct);

    private static string? NormalizeUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.Trim();

    private static void Authorize(Listing listing, string userId, bool isAdmin)
    {
        if (listing.SellerId != userId && !isAdmin)
            throw ServiceError.Forbidden();
    }
}
