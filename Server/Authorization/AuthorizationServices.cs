using System.Security.Claims;

namespace Server.Authorization;

public class AuthorizationServices
{
    private readonly UserManager<IdentityUser> _userManager;
    public AuthorizationServices(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal);
    }
    public async Task<bool> IsInRoleAsync(ClaimsPrincipal claimsPrincipal, string role)
    {
        var user = await GetUserAsync(claimsPrincipal);

        if (user is null)
            return false;

        return await _userManager.IsInRoleAsync(user, role);
    }
    public async Task<string?> GetUserIdAsync(ClaimsPrincipal claimsPrincipal)
    {
        var user = await GetUserAsync(claimsPrincipal);
        
        if (user is null)
            return null;

        return await _userManager.GetUserIdAsync(user);
    }
}
