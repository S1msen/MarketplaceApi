using MarketplaceApi.Models;
using MarketplaceApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceApi.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName.Trim()
        };
        var result = await users.CreateAsync(user, dto.Password);
        if (result.Succeeded)
            return Ok();

        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return ValidationProblem(ModelState);
    }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();
        return Ok(new CurrentUserDto(user.Id, user.Email, user.FullName, await users.GetRolesAsync(user)));
    }
}
