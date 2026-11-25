using System.ComponentModel.DataAnnotations;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Models;

public enum PriceAssessment { Unknown, Low, Normal, High }

public class PriceSuggestionQueryDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
    [Required, EnumDataType(typeof(Condition))]
    public Condition? Condition { get; set; }
}

public record PriceSuggestionDto(int CategoryId, Condition Condition, int SampleCount,
    bool HasEnoughData, decimal? SuggestedPrice);

public record CategoryStatisticsDto(int CategoryId, string CategoryName, int ActiveCount, int SoldCount,
    decimal? AverageSoldPrice, decimal? MedianSoldPrice, double? StandardDeviation,
    double? AverageSaleTimeDays, decimal? AverageDiscountPercent, decimal SaleSharePercent);

public record PriceChangeDto(int Id, int OldPrice, int NewPrice, DateTime ChangedDate);
