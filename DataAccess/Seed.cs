using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class Seed
{
    private readonly IdentityContext _identityContext;
    private readonly PersonalBudgetContext _personalBudgetContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    private readonly Item[] _incomeItems = new Item[]
    {
        new Item { Id = 1, Name = "Salary", Type = OperationType.Income },
        new Item { Id = 2, Name = "Side Hustle", Type = OperationType.Income }
    };
    private readonly Item[] _expenseItems = new Item[]
    {
        new Item { Id = 3, Name = "Rent", Type = OperationType.Expense },
        new Item { Id = 4, Name = "Groceries", Type = OperationType.Expense },
        new Item { Id = 5, Name = "Utility Bills", Type = OperationType.Expense },
        new Item { Id = 6, Name = "Entertaiments", Type = OperationType.Expense },
        new Item { Id = 7, Name = "Misc", Type = OperationType.Expense }
    };

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

    public async Task EnsureDemoPopulatedAsync()
    {
        if (await _userManager.Users.Where(u => u.UserName == "demo").AnyAsync() == false)
            await PopulateDemoAsync();
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
        await _personalBudgetContext.Items.AddRangeAsync(_incomeItems);
        await _personalBudgetContext.SaveChangesAsync();
    }

    private async Task PopulateExpenseItemsAsync()
    {
        await _personalBudgetContext.Items.AddRangeAsync(_expenseItems);
        await _personalBudgetContext.SaveChangesAsync();
    }

    private async Task PopulateDemoAsync()
    {
        var demoUser = new IdentityUser { UserName = "demo", Email = "demo@demo.com" };
        await _userManager.CreateAsync(demoUser, "321456");
        await _userManager.AddToRoleAsync(demoUser, "user");

        var currentMonth = CurrentMonthBegining();

        var operations = new Operation[]
        {
            new Operation
            {
                Type = OperationType.Income,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 1, 14, 00, 00).ToUniversalTime(),
                Item = _incomeItems.Where(i => i.Name == "Salary").First(),
                Sum = 37500,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 3, 19, 25, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Groceries").First(),
                Sum = 2711.37,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Income,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 7, 12, 30, 00).ToUniversalTime(),
                Item = _incomeItems.Where(i => i.Name == "Side Hustle").First(),
                Sum = 4250.27,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 9, 11, 10, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Groceries").First(),
                Sum = 3244.25,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 10, 20, 00, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Rent").First(),
                Sum = 32000,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Income,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 15, 14, 00, 00).ToUniversalTime(),
                Item = _incomeItems.Where(i => i.Name == "Salary").First(),
                Sum = 37500,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 17, 10, 35, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Groceries").First(),
                Sum = 5603.17,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Income,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 20, 21, 15, 00).ToUniversalTime(),
                Item = _incomeItems.Where(i => i.Name == "Side Hustle").First(),
                Sum = 5500,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 24, 21, 15, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Groceries").First(),
                Sum = 3425.5,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 25, 15, 30, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Utility Bills").First(),
                Sum = 5253.74,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 28, 12, 00, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Entertaiments").First(),
                Sum = 17254.5,
                AuthorId = demoUser.Id
            },
            new Operation
            {
                Type = OperationType.Expense,
                Date = new DateTime(currentMonth.Year, currentMonth.Month, 28, 12, 00, 00).ToUniversalTime(),
                Item = _expenseItems.Where(i => i.Name == "Misc").First(),
                Sum = 9345.1,
                AuthorId = demoUser.Id
            },
        };

        await _personalBudgetContext.Operations.AddRangeAsync(operations);
        await _personalBudgetContext.SaveChangesAsync();
    }

    private static DateTime CurrentMonthBegining()
    {
        var currentDate = DateTime.UtcNow;
        return new DateTime(currentDate.Year, currentDate.Month, 1);
    }
}
