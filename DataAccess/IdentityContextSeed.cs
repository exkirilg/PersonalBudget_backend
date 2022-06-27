using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class IdentityContextSeed
{
    private readonly IdentityContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public IdentityContextSeed(IdentityContext context, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task EnsurePopulatedAsync()
    {
        if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            await _context.Database.MigrateAsync();

        if (await _roleManager.Roles.AnyAsync() == false)
            await PopulateRoles();

        if (await _userManager.Users.AnyAsync() == false)
            await PopulateUsers();
    }

    private async Task PopulateRoles()
    {
        var roles = new List<IdentityRole>
        {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        };

        foreach (var role in roles)
            await _roleManager.CreateAsync(role);
    }

    private async Task PopulateUsers()
    {
        var admin = new IdentityUser { UserName = "admin", Email = "admin@admin.com" };
        await _userManager.CreateAsync(admin, "123456");
        await _userManager.AddToRoleAsync(admin, "admin");
        await _userManager.AddToRoleAsync(admin, "user");
    }
}
