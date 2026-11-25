using MarketplaceApi.Models;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Route("api/analytics")]
public class AnalyticsController(IAnalyticsService analytics) : ControllerBase
{
    [Authorize, HttpGet("price-suggestion")]
    public async Task<ActionResult<PriceSuggestionDto>> Suggest([FromQuery] PriceSuggestionQueryDto query,
        CancellationToken ct) =>
        Ok(await analytics.SuggestAsync(query.CategoryId, query.Condition!.Value, ct));

    [HttpGet("categories/{id:int}/stats")]
    public async Task<ActionResult<CategoryStatisticsDto>> Statistics(int id, CancellationToken ct) =>
        Ok(await analytics.GetStatisticsAsync(id, ct));

    [HttpGet("overview")]
    public async Task<ActionResult<List<CategoryStatisticsDto>>> Overview(CancellationToken ct) =>
        Ok(await analytics.GetOverviewAsync(ct));

    [HttpGet("listings/{id:int}/price-history")]
    public async Task<ActionResult<List<PriceChangeDto>>> History(int id, CancellationToken ct) =>
        Ok(await analytics.GetHistoryAsync(id, ct));
}
