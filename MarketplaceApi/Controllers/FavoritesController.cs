using System.Security.Claims;
using MarketplaceApi.Models;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Authorize, Route("api/favorites")]
public class FavoritesController(IFavoriteService favorites) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<FavoriteDto>>> GetMine(CancellationToken ct) =>
        Ok(await favorites.GetMineAsync(UserId, ct));

    [HttpPost("{listingId:int}")]
    [ProducesResponseType<FavoriteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<FavoriteDto>> Add(int listingId, CancellationToken ct)
    {
        var favorite = await favorites.AddAsync(listingId, UserId, ct);
        return favorite is null ? NoContent() : StatusCode(StatusCodes.Status201Created, favorite);
    }

    [HttpDelete("{listingId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(int listingId, CancellationToken ct)
    {
        await favorites.RemoveAsync(listingId, UserId, ct);
        return NoContent();
    }
}
