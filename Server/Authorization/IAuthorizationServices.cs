using System.Security.Claims;

namespace Server.Authorization;

public interface IAuthorizationServices
{
    public Task<IdentityUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal);
    public Task<bool> IsInRoleAsync(ClaimsPrincipal claimsPrincipal, string role);
    public Task<string?> GetUserIdAsync(ClaimsPrincipal claimsPrincipal);
}
