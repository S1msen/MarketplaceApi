using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;

namespace MarketplaceApi.Services;

internal static class DtoMapping
{
    private static readonly TimeZoneInfo NorwayTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Oslo");

    public static CategoryDto ToDto(this Category category) => new(category.Id, category.Name);

    public static ListingDto ToDto(this Listing listing) => new(
        listing.Id, listing.Title, listing.Description, listing.Price, listing.Condition, listing.Status,
        listing.ImageUrl, ToNorwayTime(listing.CreatedDate),
        listing.SoldDate is { } soldDate ? ToNorwayTime(soldDate) : null,
        listing.SoldPrice, listing.Category.ToDto(),
        listing.SellerId, listing.Seller.FullName, listing.Offers.Count);

    public static OfferDto ToDto(this Offer offer) => new(
        offer.Id, offer.ListingId, offer.Listing.Title, offer.BuyerId,
        offer.Amount, offer.Status, offer.CreatedDate);

    private static DateTimeOffset ToNorwayTime(DateTime utcDate) =>
        TimeZoneInfo.ConvertTime(new DateTimeOffset(utcDate), NorwayTimeZone);
}
