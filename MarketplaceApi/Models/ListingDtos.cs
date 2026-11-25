using System.ComponentModel.DataAnnotations;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Models;

public class ListingWriteDto
{
    [Required, StringLength(100)]
    public required string Title { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    [Range(1, int.MaxValue)]
    public int Price { get; set; }
    [Required, EnumDataType(typeof(Condition))]
    public Condition? Condition { get; set; }
    [Url, StringLength(500)]
    public string? ImageUrl { get; set; }
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class ListingQueryDto : IValidatableObject
{
    public string? Search { get; set; }
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }
    [EnumDataType(typeof(Condition))]
    public Condition? Condition { get; set; }
    [Range(1, int.MaxValue)]
    public int? MinPrice { get; set; }
    [Range(1, int.MaxValue)]
    public int? MaxPrice { get; set; }
    [RegularExpression("^(newest|price_asc|price_desc)$")]
    public string Sort { get; set; } = "newest";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice > MaxPrice)
            yield return new ValidationResult("Minimum price cannot exceed maximum price.", [nameof(MinPrice)]);
    }
}

public record ListingDto(
    int Id, string Title, string? Description, int Price, Condition Condition, ListingStatus Status,
    string? ImageUrl, DateTimeOffset CreatedDate, DateTimeOffset? SoldDate, int? SoldPrice, CategoryDto Category,
    string SellerId, string SellerName, int OfferCount, PriceAssessment? PriceAssessment = null);
