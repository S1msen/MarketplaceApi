using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MarketplaceApi.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(256)]
    public string FullName { get; set; } = null!;

    public ICollection<Listing> Listings { get; set; } = [];
    public ICollection<Offer> Offers { get; set; } = [];
    public ICollection<Favorite> Favorites { get; set; } = [];
}
