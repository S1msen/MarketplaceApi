using System.Security.Claims;
using MarketplaceApi.Controllers;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Tests;

public class ControllerTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static void SetUser(ControllerBase controller, string userId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetListingReturns200()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();
        var controller = new ListingsController(db.Listings);

        var response = await controller.Get(listing.Id, Ct);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal(listing.Id, Assert.IsType<ListingDto>(ok.Value).Id);
    }

    [Fact]
    public async Task CreateListingReturns201()
    {
        using var db = new TestDatabase();
        var controller = new ListingsController(db.Listings);
        SetUser(controller, "seller");

        var response = await controller.Create(db.WriteDto(1000), Ct);

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(ListingStatus.Active, Assert.IsType<ListingDto>(created.Value).Status);
    }

    [Fact]
    public async Task CreateOfferReturns201()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();
        var controller = new OffersController(db.Offers);
        SetUser(controller, "buyer");

        var response = await controller.Create(listing.Id, new CreateOfferDto { Amount = 600 }, Ct);

        var created = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(OfferStatus.Pending, Assert.IsType<OfferDto>(created.Value).Status);
    }

    [Fact]
    public async Task UpdateListingReturns204()
    {
        using var db = new TestDatabase();
        var listing = db.AddListing();
        var controller = new ListingsController(db.Listings);
        SetUser(controller, "seller");

        var response = await controller.Update(listing.Id, db.WriteDto(1200), Ct);

        Assert.Equal(204, Assert.IsType<NoContentResult>(response).StatusCode);
    }
}
