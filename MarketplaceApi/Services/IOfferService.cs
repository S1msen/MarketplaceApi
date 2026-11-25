using MarketplaceApi.Models;

namespace MarketplaceApi.Services;

public interface IOfferService
{
    Task<List<OfferDto>> GetForListingAsync(int listingId, string userId, CancellationToken ct);
    Task<List<OfferDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<OfferDto> CreateAsync(int listingId, CreateOfferDto dto, string userId, CancellationToken ct);
    Task AcceptAsync(int id, string userId, CancellationToken ct);
    Task RejectAsync(int id, string userId, CancellationToken ct);
    Task WithdrawAsync(int id, string userId, CancellationToken ct);
}
