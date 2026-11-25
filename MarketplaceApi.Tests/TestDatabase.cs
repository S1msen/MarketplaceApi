using MarketplaceApi.Data;
using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using MarketplaceApi.Repository;
using MarketplaceApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Tests;

public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");

    public MarketplaceDbContext Context { get; }
    public AnalyticsService Analytics { get; }
    public ListingService Listings { get; }
    public OfferService Offers { get; }
    public int CategoryId { get; }

    public TestDatabase()
    {
        connection.Open();
        Context = new MarketplaceDbContext(new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseSqlite(connection).Options);
        Context.Database.EnsureCreated();

        Context.Users.AddRange(
            new ApplicationUser { Id = "seller", UserName = "seller", FullName = "Seller" },
            new ApplicationUser { Id = "buyer", UserName = "buyer", FullName = "Buyer" },
            new ApplicationUser { Id = "other", UserName = "other", FullName = "Other buyer" });
        var category = new Category { Name = "Test category" };
        Context.Categories.Add(category);
        Context.SaveChanges();
        CategoryId = category.Id;

        var listingRepository = new ListingRepository(Context);
        var categoryRepository = new CategoryRepository(Context);
        Analytics = new AnalyticsService(listingRepository, categoryRepository);
        Listings = new ListingService(listingRepository, categoryRepository, Analytics, Context);
        Offers = new OfferService(new OfferRepository(Context), listingRepository, Context);
        Context.ChangeTracker.Clear();
    }

    public Listing AddListing(int price = 1000, ListingStatus status = ListingStatus.Active,
        int? soldPrice = null, Condition condition = Condition.New)
    {
        var listing = new Listing
        {
            Title = "Test listing", Price = price, Condition = condition, Status = status,
            CategoryId = CategoryId, SellerId = "seller", CreatedDate = DateTime.UtcNow.AddDays(-5),
            SoldDate = status == ListingStatus.Sold ? DateTime.UtcNow.AddDays(-1) : null,
            SoldPrice = status == ListingStatus.Sold ? soldPrice ?? price : null
        };
        Context.Listings.Add(listing);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
        return listing;
    }

    public ListingWriteDto WriteDto(int price) => new()
    {
        Title = "Updated listing", Price = price, Condition = Condition.New, CategoryId = CategoryId
    };

    public void Dispose()
    {
        Context.Dispose();
        connection.Dispose();
    }
}
