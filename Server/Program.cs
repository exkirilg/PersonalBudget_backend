var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebApplicationBuilder();

var app = builder.Build();
app.ConfigureWebApplication();

using (var scope = app.Services.CreateScope())
{
    var contextSeed = new IdentityContextSeed(
        scope.ServiceProvider.GetRequiredService<IdentityContext>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>());
    await contextSeed.EnsurePopulatedAsync();
}

QueriesMapping.ConfigureQueriesMapping();

app.Run();
