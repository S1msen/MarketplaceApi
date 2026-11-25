using System.Security.Claims;
using MarketplaceApi.Models;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Route("api/listings")]
public class ListingsController(IListingService listings) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<ListingDto>>> Search([FromQuery] ListingQueryDto query, CancellationToken ct) =>
        Ok(await listings.SearchAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ListingDto>> Get(int id, CancellationToken ct) =>
        Ok(await listings.GetAsync(id, ct));

    [Authorize, HttpGet("my")]
    public async Task<ActionResult<List<ListingDto>>> GetMine(CancellationToken ct) =>
        Ok(await listings.GetMineAsync(UserId, ct));

    [Authorize, HttpPost]
    [ProducesResponseType<ListingDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ListingDto>> Create(ListingWriteDto dto, CancellationToken ct)
    {
        var listing = await listings.CreateAsync(dto, UserId, ct);
        return CreatedAtAction(nameof(Get), new { id = listing.Id }, listing);
    }

    [Authorize, HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, ListingWriteDto dto, CancellationToken ct)
    {
        await listings.UpdateAsync(id, dto, UserId, User.IsInRole("Admin"), ct);
        return NoContent();
    }

    [Authorize, HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        await listings.RemoveAsync(id, UserId, User.IsInRole("Admin"), ct);
        return NoContent();
    }
}
