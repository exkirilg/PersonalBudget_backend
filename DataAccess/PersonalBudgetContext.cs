using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class PersonalBudgetContext : DbContext
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public PersonalBudgetContext(DbContextOptions<PersonalBudgetContext> options) : base(options)
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<Operation> Operations { get; set; }
}
