using MarketplaceApi.Models;

namespace MarketplaceApi.Services;

public interface IListingService
{
    Task<List<ListingDto>> SearchAsync(ListingQueryDto query, CancellationToken ct);
    Task<ListingDto> GetAsync(int id, CancellationToken ct);
    Task<List<ListingDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<ListingDto> CreateAsync(ListingWriteDto dto, string userId, CancellationToken ct);
    Task UpdateAsync(int id, ListingWriteDto dto, string userId, bool isAdmin, CancellationToken ct);
    Task RemoveAsync(int id, string userId, bool isAdmin, CancellationToken ct);
}
