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

    public async Task EnsureDatabaseMigrationsAsync()
    {
        if ((await _identityContext.Database.GetPendingMigrationsAsync()).Any())
            await _identityContext.Database.MigrateAsync();

        if ((await _personalBudgetContext.Database.GetPendingMigrationsAsync()).Any())
            await _personalBudgetContext.Database.MigrateAsync();
    }

    public async Task EnsureIdentityDbPopulatedAsync()
    {
        if (await _roleManager.Roles.AnyAsync() == false)
            await PopulateRolesAsync();

        if (await _userManager.Users.AnyAsync() == false)
            await PopulateUsersAsync();
    }

    public async Task EnsureItemsPopulatedAsync()
    {
        if (await _personalBudgetContext.Items.AnyAsync() == false)
        {
            await PopulateIncomeItemsAsync();
            await PopulateExpenseItemsAsync();
        }
    }

    private async Task PopulateRolesAsync()
    {
        var roles = new List<IdentityRole>
        {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        };

        foreach (var role in roles)
            await _roleManager.CreateAsync(role);
    }

    private async Task PopulateUsersAsync()
    {
        var admin = new IdentityUser { UserName = "admin", Email = "admin@admin.com" };
        await _userManager.CreateAsync(admin, "123456");
        await _userManager.AddToRoleAsync(admin, "admin");
        await _userManager.AddToRoleAsync(admin, "user");
    }

    private async Task PopulateIncomeItemsAsync()
    {
        var items = new Item[]
        {
            new Item { Name = "Salary", Type = OperationType.Income },
            new Item { Name = "Side Hustle", Type = OperationType.Income }
        };

        await _personalBudgetContext.Items.AddRangeAsync(items);
        await _personalBudgetContext.SaveChangesAsync();
    }

    private async Task PopulateExpenseItemsAsync()
    {
        var items = new Item[]
        {
            new Item { Name = "Rent", Type = OperationType.Expense },
            new Item { Name = "Groceries", Type = OperationType.Expense },
            new Item { Name = "Utility Bills", Type = OperationType.Expense },
            new Item { Name = "Entertaiments", Type = OperationType.Expense },
            new Item { Name = "Misc", Type = OperationType.Expense }
        };

        await _personalBudgetContext.Items.AddRangeAsync(items);
        await _personalBudgetContext.SaveChangesAsync();
    }
}
