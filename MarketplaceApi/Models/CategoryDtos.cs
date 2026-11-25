using System.ComponentModel.DataAnnotations;

namespace MarketplaceApi.Models;

public class CategoryWriteDto
{
    [Required, StringLength(50)]
    public required string Name { get; set; }
}

public record CategoryDto(int Id, string Name);
