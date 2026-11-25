using MarketplaceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MarketplaceApi.Data;

public class ApplicationUserStore(MarketplaceDbContext context, IdentityErrorDescriber describer)
    : UserStore<ApplicationUser, IdentityRole, MarketplaceDbContext>(context, describer)
{
    public override Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(user.FullName))
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "FullNameRequired",
                Description = "Full name is required."
            }));

        return Context.Database.CurrentTransaction is null
            ? Context.WriteAsync(CreateWithRoleAsync, cancellationToken)
            : CreateWithRoleAsync();

        async Task<IdentityResult> CreateWithRoleAsync()
        {
            var result = await base.CreateAsync(user, cancellationToken);
            if (!result.Succeeded)
                return result;

            Context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = "role-user" });
            await Context.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}
