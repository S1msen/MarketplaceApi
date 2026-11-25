namespace MarketplaceApi.Models.Entities;

public class PriceChange
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public Listing Listing { get; set; } = null!;
    public int OldPrice { get; set; }
    public int NewPrice { get; set; }
    public DateTime ChangedDate { get; set; }
}
