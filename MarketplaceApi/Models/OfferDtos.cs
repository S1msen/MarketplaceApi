using System.ComponentModel.DataAnnotations;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Models;

public class CreateOfferDto
{
    [Range(1, int.MaxValue)]
    public int Amount { get; set; }
}

public record OfferDto(int Id, int ListingId, string ListingTitle, string BuyerId,
    int Amount, OfferStatus Status, DateTime CreatedDate);
