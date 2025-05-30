using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using SharedTravelBG.Models;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
	public CustomUserClaimsPrincipalFactory(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager,
		IOptions<IdentityOptions> optionsAccessor)
		: base(userManager, roleManager, optionsAccessor)
	{
	}

	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
	{
		var identity = await base.GenerateClaimsAsync(user);
		// Add the FullName claim if available
		if (!string.IsNullOrEmpty(user.FullName))
		{
			identity.AddClaim(new Claim("FullName", user.FullName));
		}
		return identity;
	}
}
