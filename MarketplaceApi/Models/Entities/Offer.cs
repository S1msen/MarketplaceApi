using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.Entities;

public class Offer
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    public required string BuyerId { get; set; }
    public ApplicationUser Buyer { get; set; } = null!;
    [Range(1, int.MaxValue)]
    public int Amount { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
