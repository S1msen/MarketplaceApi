using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models.Entities;

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public required string Name { get; set; }
    public ICollection<Listing> Listings { get; set; } = [];
}
