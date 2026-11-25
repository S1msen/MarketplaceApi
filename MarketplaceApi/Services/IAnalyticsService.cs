using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Services;

public interface IAnalyticsService
{
    Task<PriceSuggestionDto> SuggestAsync(int categoryId, Condition condition, CancellationToken ct);
    Task<PriceAssessment> AssessAsync(Listing listing, CancellationToken ct);
    Task<CategoryStatisticsDto> GetStatisticsAsync(int categoryId, CancellationToken ct);
    Task<List<CategoryStatisticsDto>> GetOverviewAsync(CancellationToken ct);
    Task<List<PriceChangeDto>> GetHistoryAsync(int listingId, CancellationToken ct);
}
