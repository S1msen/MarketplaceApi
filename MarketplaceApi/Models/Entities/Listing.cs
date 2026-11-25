using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.Entities;

public class Listing
{
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public required string Title { get; set; }
    [MaxLength(2000)]
    public string? Description { get; set; }
    [Range(1, int.MaxValue)]
    public int Price { get; set; }
    public Condition Condition { get; set; }
    public ListingStatus Status { get; set; }
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? SoldDate { get; set; }
    public int? SoldPrice { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public required string SellerId { get; set; }
    public ApplicationUser Seller { get; set; } = null!;
    public ICollection<Offer> Offers { get; set; } = [];
    public ICollection<PriceChange> PriceChanges { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
}
