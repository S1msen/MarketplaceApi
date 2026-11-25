using MarketplaceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceApi.Data;

public class MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<PriceChange> PriceChanges => Set<PriceChange>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN",
                ConcurrencyStamp = "f50b505b-1de5-4fdd-b980-c1976a2716f3"
            },
            new IdentityRole
            {
                Id = "role-user", Name = "User", NormalizedName = "USER",
                ConcurrencyStamp = "e1c3846b-975c-433d-97a9-cadd7cdf185b"
            });

        builder.Entity<Listing>().HasOne(x => x.Category).WithMany(x => x.Listings)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Listing>().HasOne(x => x.Seller).WithMany(x => x.Listings)
            .HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Offer>().HasOne(x => x.Listing).WithMany(x => x.Offers)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Offer>().HasOne(x => x.Buyer).WithMany(x => x.Offers)
            .HasForeignKey(x => x.BuyerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<PriceChange>().HasOne(x => x.Listing).WithMany(x => x.PriceChanges)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Favorite>().HasOne(x => x.Listing).WithMany(x => x.Favorites)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Favorite>().HasOne(x => x.User).WithMany(x => x.Favorites)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Favorite>().HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
        builder.Entity<Offer>().HasIndex(x => new { x.ListingId, x.BuyerId })
            .IsUnique().HasFilter("\"Status\" = 0");
        builder.Entity<Listing>().HasIndex(x => new { x.CategoryId, x.Status, x.CreatedDate });
        builder.Entity<Listing>().HasIndex(x => new { x.SellerId, x.CreatedDate });
        builder.Entity<PriceChange>().HasIndex(x => new { x.ListingId, x.ChangedDate });
        builder.Entity<Listing>().Property(x => x.CreatedDate)
            .HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        builder.Entity<Listing>().Property(x => x.SoldDate)
            .HasConversion(x => x, x => x.HasValue ? DateTime.SpecifyKind(x.Value, DateTimeKind.Utc) : null);
        builder.Entity<Offer>().Property(x => x.CreatedDate)
            .HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        builder.Entity<PriceChange>().Property(x => x.ChangedDate)
            .HasConversion(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc));
        builder.Entity<Listing>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Listing_Price", "\"Price\" > 0");
            t.HasCheckConstraint("CK_Listing_Condition", "\"Condition\" BETWEEN 0 AND 4");
            t.HasCheckConstraint("CK_Listing_Status", "\"Status\" BETWEEN 0 AND 2");
            t.HasCheckConstraint("CK_Listing_Sale",
                "(\"Status\" = 1 AND \"SoldPrice\" IS NOT NULL AND \"SoldPrice\" > 0 AND \"SoldDate\" IS NOT NULL AND \"SoldDate\" >= \"CreatedDate\") OR " +
                "(\"Status\" <> 1 AND \"SoldPrice\" IS NULL AND \"SoldDate\" IS NULL)");
        });
        builder.Entity<Offer>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Offer_Amount", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_Offer_Status", "\"Status\" BETWEEN 0 AND 3");
        });
        builder.Entity<PriceChange>().ToTable(t =>
            t.HasCheckConstraint("CK_PriceChange_Prices", "\"OldPrice\" > 0 AND \"NewPrice\" > 0"));
    }

    public async Task<T> WriteAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await Database.BeginTransactionAsync(ct);
        var result = await action();
        await SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return result;
    }
}
