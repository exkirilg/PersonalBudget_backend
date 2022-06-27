using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class Seed
{
    private readonly IdentityContext _identityContext;
    private readonly PersonalBudgetContext _personalBudgetContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public Seed(
        IdentityContext identityContext, PersonalBudgetContext personalBudgetContext,
        RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _identityContext = identityContext;
        _personalBudgetContext = personalBudgetContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task EnsureDatabaseMigrations()
    {
        if ((await _identityContext.Database.GetPendingMigrationsAsync()).Any())
            await _identityContext.Database.MigrateAsync();

        if ((await _personalBudgetContext.Database.GetPendingMigrationsAsync()).Any())
            await _personalBudgetContext.Database.MigrateAsync();
    }

    public async Task EnsureIdentityDbPopulatedAsync()
    {
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
