using MarketplaceApi.Models;
using MarketplaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Route("api/categories")]
public class CategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct) =>
        Ok(await categories.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Get(int id, CancellationToken ct) =>
        Ok(await categories.GetAsync(id, ct));

    [Authorize(Roles = "Admin"), HttpPost]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CategoryDto>> Create(CategoryWriteDto dto, CancellationToken ct)
    {
        var category = await categories.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }

    [Authorize(Roles = "Admin"), HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, CategoryWriteDto dto, CancellationToken ct)
    {
        await categories.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await categories.DeleteAsync(id, ct);
        return NoContent();
    }
}
