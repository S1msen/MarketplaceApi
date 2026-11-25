using System.Security.Claims;
using MarketplaceApi.Models;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Authorize, Route("api/offers")]
public class OffersController(IOfferService offers) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("/api/listings/{id:int}/offers")]
    public async Task<ActionResult<List<OfferDto>>> GetForListing(int id, CancellationToken ct) =>
        Ok(await offers.GetForListingAsync(id, UserId, ct));

    [HttpPost("/api/listings/{id:int}/offers")]
    [ProducesResponseType<OfferDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OfferDto>> Create(int id, CreateOfferDto dto, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await offers.CreateAsync(id, dto, UserId, ct));

    [HttpGet("my")]
    public async Task<ActionResult<List<OfferDto>>> GetMine(CancellationToken ct) =>
        Ok(await offers.GetMineAsync(UserId, ct));

    [HttpPost("{id:int}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Accept(int id, CancellationToken ct)
    {
        await offers.AcceptAsync(id, UserId, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        await offers.RejectAsync(id, UserId, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Withdraw(int id, CancellationToken ct)
    {
        await offers.WithdrawAsync(id, UserId, ct);
        return NoContent();
    }
}
