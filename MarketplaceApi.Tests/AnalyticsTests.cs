using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Services;

namespace MarketplaceApi.Tests;

public class AnalyticsTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Theory]
    [InlineData(new int[] { 100, 300, 200 }, 200)]
    [InlineData(new int[] { 400, 100, 300, 200 }, 250)]
    public async Task MedianHandlesOddAndEvenNumbersOfSales(int[] prices, int expected)
    {
        using var db = new TestDatabase();
        foreach (var price in prices)
            db.AddListing(status: ListingStatus.Sold, soldPrice: price);

        var stats = await db.Analytics.GetStatisticsAsync(db.CategoryId, Ct);

        Assert.Equal((decimal)expected, stats.MedianSoldPrice);
    }

    [Fact]
    public void QuartilesMatchExcelQuartileIncForGivenDataSet()
    {
        decimal[] prices = [10, 20, 30, 40, 50, 60];

        Assert.Equal(22.5m, AnalyticsService.Percentile(prices, 0.25m));
        Assert.Equal(47.5m, AnalyticsService.Percentile(prices, 0.75m));
    }

    [Fact]
    public async Task SuggestionNormalizesConditionsAndRoundsToNearestTen()
    {
        using var db = new TestDatabase();
        db.AddListing(status: ListingStatus.Sold, soldPrice: 1000, condition: Condition.New);
        db.AddListing(status: ListingStatus.Sold, soldPrice: 770, condition: Condition.Good);
        db.AddListing(status: ListingStatus.Sold, soldPrice: 660, condition: Condition.Used);

        var result = await db.Analytics.SuggestAsync(db.CategoryId, Condition.Used, Ct);

        // Normalized prices are 1000, 1100 and 1200; 1100 * 0.55 = 605
        Assert.True(result.HasEnoughData);
        Assert.Equal(3, result.SampleCount);
        Assert.Equal(610m, result.SuggestedPrice);
    }

    [Fact]
    public async Task SuggestionWithFewerThanThreeSalesHasNoPrice()
    {
        using var db = new TestDatabase();
        db.AddListing(status: ListingStatus.Sold, soldPrice: 1000);
        db.AddListing(status: ListingStatus.Sold, soldPrice: 1200);
        db.AddListing();

        var result = await db.Analytics.SuggestAsync(db.CategoryId, Condition.New, Ct);

        Assert.False(result.HasEnoughData);
        Assert.Equal(2, result.SampleCount);
        Assert.Null(result.SuggestedPrice);
    }

    [Theory]
    [InlineData(49, PriceAssessment.Low)]
    [InlineData(50, PriceAssessment.Normal)]
    [InlineData(150, PriceAssessment.Normal)]
    [InlineData(250, PriceAssessment.Normal)]
    [InlineData(251, PriceAssessment.High)]
    public async Task AssessmentUsesInclusiveQuartileFences(int price, PriceAssessment expected)
    {
        using var db = new TestDatabase();
        foreach (var soldPrice in new[] { 100, 120, 140, 160, 180, 200 })
            db.AddListing(status: ListingStatus.Sold, soldPrice: soldPrice);
        var listing = db.AddListing(price: price);

        // Q1 = 125, Q3 = 175, IQR = 50; the fences are 50 and 250.
        var result = await db.Analytics.AssessAsync(listing, Ct);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AssessmentWithFewerThanFiveSalesIsUnknown()
    {
        using var db = new TestDatabase();
        for (var i = 0; i < 4; i++)
            db.AddListing(status: ListingStatus.Sold, soldPrice: 100 + i * 10);
        var listing = db.AddListing(price: 5000);

        Assert.Equal(PriceAssessment.Unknown, await db.Analytics.AssessAsync(listing, Ct));
    }

    [Theory]
    [InlineData(34, PriceAssessment.Low)]
    [InlineData(35, PriceAssessment.Normal)]
    [InlineData(175, PriceAssessment.Normal)]
    [InlineData(176, PriceAssessment.High)]
    public async Task AssessmentNormalizesListingCondition(int price, PriceAssessment expected)
    {
        using var db = new TestDatabase();
        foreach (var soldPrice in new[] { 100, 120, 140, 160, 180, 200 })
            db.AddListing(status: ListingStatus.Sold, soldPrice: soldPrice);
        var listing = db.AddListing(price: price, condition: Condition.Good);

        Assert.Equal(expected, await db.Analytics.AssessAsync(listing, Ct));
    }

    [Fact]
    public async Task AverageDiscountUsesFirstOriginalPrice()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing(price: 1000, status: ListingStatus.Sold, soldPrice: 1000);
        db.Context.PriceChanges.Add(new PriceChange
        {
            ListingId = listing.Id, OldPrice = 2000, NewPrice = 1000,
            ChangedDate = DateTime.UtcNow.AddDays(-3)
        });
        await db.Context.SaveChangesAsync();
        db.AddListing(price: 1000, status: ListingStatus.Sold, soldPrice: 800);

        var stats = await db.Analytics.GetStatisticsAsync(db.CategoryId, Ct);

        // Discounts are 50% from 2000 to 1000 and 20% from 1000 to 800.
        Assert.Equal(35m, stats.AverageDiscountPercent);
    }
}
