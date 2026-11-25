using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;

namespace MarketplaceApi.Services;

public class AnalyticsService(IListingRepository listings, ICategoryRepository categories) : IAnalyticsService
{
    public async Task<PriceSuggestionDto> SuggestAsync(int categoryId, Condition condition, CancellationToken ct)
    {
        _ = await categories.GetAsync(categoryId, ct) ?? throw ServiceError.Missing("Category");
        var prices = NormalizedSales(await listings.GetAnalyticsDataAsync(categoryId, ct));
        return new PriceSuggestionDto(categoryId, condition, prices.Length, prices.Length >= 3,
            prices.Length < 3 ? null :
                Math.Round(Percentile(prices, 0.5m) * Factor(condition) / 10m, 0, MidpointRounding.AwayFromZero) * 10m);
    }

    public async Task<PriceAssessment> AssessAsync(Listing listing, CancellationToken ct)
    {
        var prices = NormalizedSales(await listings.GetAnalyticsDataAsync(listing.CategoryId, ct));
        if (prices.Length < 5)
            return PriceAssessment.Unknown;
        var q1 = Percentile(prices, 0.25m);
        var q3 = Percentile(prices, 0.75m);
        var iqr = q3 - q1;
        var normalized = listing.Price / Factor(listing.Condition);
        return normalized < q1 - 1.5m * iqr ? PriceAssessment.Low :
            normalized > q3 + 1.5m * iqr ? PriceAssessment.High : PriceAssessment.Normal;
    }

    public async Task<CategoryStatisticsDto> GetStatisticsAsync(int categoryId, CancellationToken ct)
    {
        var category = await categories.GetAsync(categoryId, ct) ?? throw ServiceError.Missing("Category");
        return Statistics(category, await listings.GetAnalyticsDataAsync(categoryId, ct));
    }

    public async Task<List<CategoryStatisticsDto>> GetOverviewAsync(CancellationToken ct)
    {
        var allCategories = await categories.GetAllAsync(ct);
        var data = (await listings.GetAnalyticsDataAsync(null, ct)).ToLookup(x => x.CategoryId);
        return allCategories.Select(x => Statistics(x, data[x.Id].ToList())).ToList();
    }

    public async Task<List<PriceChangeDto>> GetHistoryAsync(int listingId, CancellationToken ct)
    {
        var listing = await listings.GetAsync(listingId, ct) ?? throw ServiceError.Missing("Listing");
        return listing.PriceChanges.OrderBy(x => x.ChangedDate).ThenBy(x => x.Id)
            .Select(x => new PriceChangeDto(x.Id, x.OldPrice, x.NewPrice, x.ChangedDate)).ToList();
    }

    private static decimal Factor(Condition condition) => condition switch
    {
        Condition.New => 1m,
        Condition.LikeNew => 0.85m,
        Condition.Good => 0.70m,
        Condition.Used => 0.55m,
        Condition.Defect => 0.30m,
        _ => throw ServiceError.Invalid("Unknown condition.")
    };

    private static decimal[] NormalizedSales(IEnumerable<Listing> data) =>
        data.Where(x => x.Status == ListingStatus.Sold)
            .Select(x => x.SoldPrice!.Value / Factor(x.Condition)).Order().ToArray();

    internal static decimal Percentile(decimal[] sorted, decimal percentile)
    {
        var position = (sorted.Length - 1) * percentile;
        var lower = (int)decimal.Floor(position);
        var upper = (int)decimal.Ceiling(position);
        return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
    }

    private static CategoryStatisticsDto Statistics(Category category, List<Listing> data)
    {
        var sold = data.Where(x => x.Status == ListingStatus.Sold).ToArray();
        var prices = sold.Select(x => (decimal)x.SoldPrice!.Value).Order().ToArray();
        decimal? mean = prices.Length == 0 ? null : prices.Average();
        double? deviation = prices.Length < 2 ? null :
            Math.Sqrt((double)(prices.Sum(x => (x - mean!.Value) * (x - mean.Value)) / (prices.Length - 1)));
        return new CategoryStatisticsDto(category.Id, category.Name,
            data.Count(x => x.Status == ListingStatus.Active), sold.Length,
            mean, prices.Length == 0 ? null : Percentile(prices, 0.5m), deviation,
            sold.Length == 0 ? null : sold.Average(x => (x.SoldDate!.Value - x.CreatedDate).TotalDays),
            sold.Length == 0 ? null : sold.Average(x =>
            {
                var original = x.PriceChanges.OrderBy(c => c.ChangedDate).ThenBy(c => c.Id)
                    .Select(c => (int?)c.OldPrice).FirstOrDefault() ?? x.Price;
                return (original - (decimal)x.SoldPrice!.Value) / original * 100m;
            }),
            data.Count == 0 ? 0m : sold.Length * 100m / data.Count);
    }
}
