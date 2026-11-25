using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;

namespace MarketplaceApi.Services;

public class OfferService(IOfferRepository offers, IListingRepository listings, MarketplaceDbContext context)
    : IOfferService
{
    public async Task<List<OfferDto>> GetForListingAsync(int listingId, string userId, CancellationToken ct)
    {
        var listing = await listings.GetAsync(listingId, ct) ?? throw ServiceError.Missing("Listing");
        if (listing.SellerId != userId)
            throw ServiceError.Forbidden();
        return listing.Offers.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.Id)
            .Select(x => x.ToDto()).ToList();
    }

    public async Task<List<OfferDto>> GetMineAsync(string userId, CancellationToken ct) =>
        (await offers.GetMineAsync(userId, ct)).Select(x => x.ToDto()).ToList();

    public Task<OfferDto> CreateAsync(int listingId, CreateOfferDto dto, string userId, CancellationToken ct) =>
        context.WriteAsync(async () =>
        {
            var listing = await listings.GetAsync(listingId, ct) ?? throw ServiceError.Missing("Listing");
            if (listing.SellerId == userId)
                throw ServiceError.Invalid("You cannot make an offer on your own listing.");
            if (listing.Status != ListingStatus.Active)
                throw ServiceError.Conflict("Offers require an active listing.");
            if (listing.Offers.Any(x => x.BuyerId == userId && x.Status == OfferStatus.Pending))
                throw ServiceError.Conflict("You already have a pending offer on this listing.");
            if ((long)dto.Amount * 2 < listing.Price)
                throw ServiceError.Invalid("An offer must be at least 50% of the listing price.");
            var offer = new Offer
            {
                Listing = listing, BuyerId = userId, Amount = dto.Amount,
                Status = OfferStatus.Pending, CreatedDate = DateTime.UtcNow
            };
            offers.Add(offer);
            await context.SaveChangesAsync(ct);
            return offer.ToDto();
        }, ct);

    public Task AcceptAsync(int id, string userId, CancellationToken ct) =>
        TransitionAsync(id, userId, OfferStatus.Accepted, ct);

    public Task RejectAsync(int id, string userId, CancellationToken ct) =>
        TransitionAsync(id, userId, OfferStatus.Rejected, ct);

    public Task WithdrawAsync(int id, string userId, CancellationToken ct) =>
        TransitionAsync(id, userId, OfferStatus.Withdrawn, ct);

    private async Task TransitionAsync(int id, string userId, OfferStatus target, CancellationToken ct) =>
        await context.WriteAsync(async () =>
        {
            var offer = await offers.GetAsync(id, ct) ?? throw ServiceError.Missing("Offer");
            var ownerId = target == OfferStatus.Withdrawn ? offer.BuyerId : offer.Listing.SellerId;
            if (ownerId != userId)
                throw ServiceError.Forbidden();
            if (offer.Status != OfferStatus.Pending)
                throw ServiceError.Conflict("Only pending offers can change status.");
            if (offer.Listing.Status != ListingStatus.Active)
                throw ServiceError.Conflict("The listing is no longer active.");
            offer.Status = target;
            if (target == OfferStatus.Accepted)
            {
                offer.Listing.Status = ListingStatus.Sold;
                offer.Listing.SoldPrice = offer.Amount;
                offer.Listing.SoldDate = DateTime.UtcNow;
                foreach (var other in offer.Listing.Offers.Where(x => x.Id != offer.Id && x.Status == OfferStatus.Pending))
                    other.Status = OfferStatus.Rejected;
            }
            return true;
        }, ct);
}
