using DataAccess.Repositories;
using DbUp;
using Domain.Interfaces.DataAccess;

namespace Server;

public static class AppConfigurationExtensions
{
    public static void ConfigureWebApplicationBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();

        #region Swagger/OpenAPI

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        #endregion

        #region Data access

        var connectionString = builder.Configuration["ConnectionStrings:PersonalBudgetConnection"];
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString, null)
            .WithScriptsEmbeddedInAssembly(
                System.Reflection.Assembly.GetExecutingAssembly(),
                (string scripts) => scripts.StartsWith("Server.PostgreSQLScripts")
            )
            .WithTransaction()
            .Build();

        if (upgrader.IsUpgradeRequired())
            upgrader.PerformUpgrade();

        builder.Services.AddScoped<IBudgetItemsRepository, BudgetItemsRepository>();
        builder.Services.AddScoped<IBudgetOperationsRepository, BudgetOperationsRepository>();

        #endregion
    }
    public static void ConfigureWebApplication(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
