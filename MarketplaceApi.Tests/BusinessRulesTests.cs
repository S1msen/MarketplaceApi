using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Tests;

public class BusinessRulesTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task SellerCannotOfferOnOwnListing()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();

        var error = await Assert.ThrowsAsync<ServiceError>(() =>
            db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 600 }, "seller", Ct));

        Assert.Equal(ErrorKind.Invalid, error.Kind);
        Assert.Empty(await db.Context.Offers.ToListAsync());
    }

    [Fact]
    public async Task SoldListingRejectsOffers()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing(status: ListingStatus.Sold);

        var error = await Assert.ThrowsAsync<ServiceError>(() =>
            db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 600 }, "buyer", Ct));

        Assert.Equal(ErrorKind.Conflict, error.Kind);
        Assert.Empty(await db.Context.Offers.ToListAsync());
    }

    [Fact]
    public async Task BuyerCannotMakeSecondPendingOffer()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();
        await db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 600 }, "buyer", Ct);

        var error = await Assert.ThrowsAsync<ServiceError>(() =>
            db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 700 }, "buyer", Ct));

        Assert.Equal(ErrorKind.Conflict, error.Kind);
        Assert.Single(await db.Context.Offers.ToListAsync());
    }

    [Fact]
    public async Task OfferBelowHalfIsRejectedAndExactlyHalfIsAccepted()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();

        var error = await Assert.ThrowsAsync<ServiceError>(() =>
            db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 499 }, "buyer", Ct));
        Assert.Equal(ErrorKind.Invalid, error.Kind);
        var offer = await db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 500 }, "buyer", Ct);

        Assert.Equal(500, offer.Amount);
        Assert.Equal(OfferStatus.Pending, offer.Status);
    }

    [Fact]
    public async Task AcceptingOfferSellsListingAndRejectsOtherPendingOffers()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();
        var accepted = await db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 800 }, "buyer", Ct);
        var rejected = await db.Offers.CreateAsync(listing.Id, new CreateOfferDto { Amount = 700 }, "other", Ct);

        await db.Offers.AcceptAsync(accepted.Id, "seller", Ct);
        db.Context.ChangeTracker.Clear();
        var saved = await db.Context.Listings.Include(x => x.Offers).SingleAsync(x => x.Id == listing.Id);

        Assert.Equal(ListingStatus.Sold, saved.Status);
        Assert.Equal(800, saved.SoldPrice);
        Assert.NotNull(saved.SoldDate);
        Assert.Equal(OfferStatus.Accepted, saved.Offers.Single(x => x.Id == accepted.Id).Status);
        Assert.Equal(OfferStatus.Rejected, saved.Offers.Single(x => x.Id == rejected.Id).Status);
    }

    [Fact]
    public async Task ChangingPriceRecordsOldAndNewPrice()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing(price: 1000);

        await db.Listings.UpdateAsync(listing.Id, db.WriteDto(1200), "seller", false, Ct);
        db.Context.ChangeTracker.Clear();
        var change = await db.Context.PriceChanges.SingleAsync(x => x.ListingId == listing.Id);
        var updated = await db.Context.Listings.SingleAsync(x => x.Id == listing.Id);

        Assert.Equal(1000, change.OldPrice);
        Assert.Equal(1200, change.NewPrice);
        Assert.Equal(1200, updated.Price);
    }

    [Fact]
    public async Task SoldListingCannotBeEdited()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing(status: ListingStatus.Sold);

        var error = await Assert.ThrowsAsync<ServiceError>(() =>
            db.Listings.UpdateAsync(listing.Id, db.WriteDto(1200), "seller", false, Ct));

        Assert.Equal(ErrorKind.Conflict, error.Kind);
        db.Context.ChangeTracker.Clear();
        Assert.Equal(1000, (await db.Context.Listings.SingleAsync(x => x.Id == listing.Id)).Price);
        Assert.Empty(await db.Context.PriceChanges.ToListAsync());
    }
}
