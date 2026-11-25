using MarketplaceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var context = services.GetRequiredService<MarketplaceDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        await context.WriteAsync(async () =>
        {
            string[] emails = ["admin@brukt.no", "ola@brukt.no", "kari@brukt.no", "per@brukt.no"];
            string[] names = ["Marketplace Admin", "Ola Nordmann", "Kari Trå", "Per Hansen"];
            string[] passwordHashes =
            [
                "AQAAAAIAAYagAAAAEOcS0LrrViCMBX4AXcXhKhdrnnzadYAJDR3h2lrhIYKJXZDMiUmK787HquuzGb9hvA==",
                "AQAAAAIAAYagAAAAEF9+Fw+pP/EzVuP8/GN5Q3r+e6INlM5g7h9L8rC+7ZFr9yfW6ZCtyTY513wiAF3BmA==",
                "AQAAAAIAAYagAAAAEGQelAF/o97jqv2/xOuWhVdS2oX8Zba75aoXDY6Qy9d3vsDQ2W9g6ZNrVaOq9k7fIw==",
                "AQAAAAIAAYagAAAAEE/69Dr/+ChP9YyxuG7O0CInkTkepFImth5F9n9u+wy8KVYsGqWsSGhH8P1i4cqy6w=="
            ];
            var accounts = new List<ApplicationUser>();
            for (var i = 0; i < emails.Length; i++)
            {
                var user = await users.FindByEmailAsync(emails[i]);
                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        Id = $"demo-{i}", UserName = emails[i], Email = emails[i],
                        FullName = names[i], EmailConfirmed = true, PasswordHash = passwordHashes[i]
                    };
                    Check(await users.CreateAsync(user));
                    if (i == 0)
                        Check(await users.AddToRoleAsync(user, "Admin"));
                }
                accounts.Add(user);
            }

            if (await context.Listings.AnyAsync(ct))
                return true;

            string[] categoryNames = ["Elektronikk", "Sykkel", "Møbler", "Friluft"];
            string[][] listingTitles =
            [
                ["Mobiltelefon", "Nettbrett", "Bærbar PC", "Skjerm", "Tastatur", "Hodetelefoner",
                    "Kamera", "Spillkonsoll", "Smartklokke", "Høyttaler", "Ruter", "Mikrofon"],
                ["Terrengsykkel", "Bysykkel", "Landeveissykkel", "Barnesykkel", "Elsykkel",
                    "Gravelsykkel", "Hybridsykkel", "Sammenleggbar sykkel", "BMX", "Tursykkel"],
                ["Skrivebord", "Spisebord", "Kontorstol", "Lenestol", "Bokhylle",
                    "Sofa", "Kommode", "Spisestol", "Sengebord", "Gulvlampe"],
                ["Telt", "Sovepose", "Ryggsekk", "Turstaver", "Liggeunderlag",
                    "Stormkjøkken", "Hengekøye", "Turjakke"]
            ];
            string[] conditionNames = ["som ny", "nesten som ny", "god", "brukt", "med mangler"];
            int[] soldCounts = [8, 6, 5, 2];
            int[] activeCounts = [3, 3, 4, 5];
            var epoch = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
            var sequence = 0;
            for (var c = 0; c < categoryNames.Length; c++)
            {
                var categoryName = categoryNames[c];
                var category = await context.Categories.SingleOrDefaultAsync(x => x.Name == categoryName, ct);
                if (category is null)
                {
                    category = new Category { Name = categoryName };
                    context.Categories.Add(category);
                }
                var total = soldCounts[c] + activeCounts[c] + 1;
                for (var i = 0; i < total; i++)
                {
                    var seller = accounts[1 + sequence % 3];
                    var buyer = accounts[1 + (sequence + 1) % 3];
                    var otherBuyer = accounts[1 + (sequence + 2) % 3];
                    var condition = (Condition)(i % 5);
                    decimal[] factors = [1m, 0.85m, 0.70m, 0.55m, 0.30m];
                    var soldPrice = (int)((1000 + c * 500 + i * 100) * factors[(int)condition]);
                    var isSold = i < soldCounts[c];
                    var isRemoved = i == total - 1;
                    var askingPrice = isSold ? soldPrice + 200
                        : (int)Math.Round((1000 + c * 500 + i % 5 * 100) * factors[(int)condition] * 1.1m);
                    var created = epoch.AddDays(sequence);
                    var listing = new Listing
                    {
                        Title = $"{listingTitles[c][i]} #{i + 1}",
                        Description = $"Brukt vare i kategorien {categoryName.ToLowerInvariant()}. Tilstand: {conditionNames[(int)condition]}.",
                        Category = category, SellerId = seller.Id, Condition = condition,
                        ImageUrl = null,
                        Price = c == 0 && i == soldCounts[c] ? 50000 : askingPrice,
                        Status = isSold ? ListingStatus.Sold : isRemoved ? ListingStatus.Removed : ListingStatus.Active,
                        CreatedDate = created,
                        SoldDate = isSold ? created.AddDays(2 + i).AddHours(12) : null,
                        SoldPrice = isSold ? soldPrice : null
                    };
                    if (i < 2)
                    {
                        listing.PriceChanges.Add(new PriceChange
                        {
                            OldPrice = listing.Price + 500, NewPrice = listing.Price + 200,
                            ChangedDate = created.AddHours(6)
                        });
                        listing.PriceChanges.Add(new PriceChange
                        {
                            OldPrice = listing.Price + 200, NewPrice = listing.Price,
                            ChangedDate = created.AddDays(1)
                        });
                    }
                    if (isSold)
                    {
                        listing.Offers.Add(new Offer
                        {
                            BuyerId = buyer.Id, Amount = soldPrice, Status = OfferStatus.Accepted,
                            CreatedDate = created.AddDays(2)
                        });
                        listing.Offers.Add(new Offer
                        {
                            BuyerId = otherBuyer.Id, Amount = soldPrice, Status = OfferStatus.Rejected,
                            CreatedDate = created.AddDays(1)
                        });
                    }
                    else if (!isRemoved && i == soldCounts[c])
                    {
                        listing.Offers.Add(new Offer
                        {
                            BuyerId = buyer.Id, Amount = listing.Price, Status = OfferStatus.Pending,
                            CreatedDate = created.AddDays(1)
                        });
                        listing.Offers.Add(new Offer
                        {
                            BuyerId = otherBuyer.Id, Amount = listing.Price, Status = OfferStatus.Withdrawn,
                            CreatedDate = created.AddHours(12)
                        });
                    }
                    context.Listings.Add(listing);
                    if (i % 3 == 0)
                        context.Favorites.Add(new Favorite { Listing = listing, UserId = buyer.Id });
                    sequence++;
                }
            }
            return true;
        }, ct);
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Demo account seeding failed: " +
                string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
