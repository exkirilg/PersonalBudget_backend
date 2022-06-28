var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebApplicationBuilder();

var app = builder.Build();
app.ConfigureWebApplication();

using (var scope = app.Services.CreateScope())
{
    var seed = new Seed(
        scope.ServiceProvider.GetRequiredService<IdentityContext>(),
        scope.ServiceProvider.GetRequiredService<PersonalBudgetContext>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>());
    await seed.EnsureDatabaseMigrationsAsync();
    await seed.EnsureIdentityDbPopulatedAsync();
    await seed.EnsureItemsPopulatedAsync();
}

app.Run();
