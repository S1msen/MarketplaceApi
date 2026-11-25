using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models;

public class RegisterDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required, StringLength(256)]
    public required string FullName { get; set; }
}

public record CurrentUserDto(string Id, string? Email, string FullName, IList<string> Roles);
