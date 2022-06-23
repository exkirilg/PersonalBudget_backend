using DataAccess.Repositories;
using DbUp;
using Domain.Interfaces.Cache;
using Domain.Interfaces.DataAccess;
using Domain.Models.Cache;
using Microsoft.OpenApi.Models;

namespace Server;

public static class AppConfigurationExtensions
{
    public static void ConfigureWebApplicationBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        
        builder.Services.AddHttpClient();

        #region Swagger/OpenAPI

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

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

        #region Cache

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IBudgetItemsCache, BudgetItemsCache>();
        builder.Services.AddSingleton<IBudgetOperationsCache, BudgetOperationsCache>();

        #endregion

        #region CORS

        builder.Services.AddCors(
            options => options.AddPolicy("CorsPolicy", policy => policy
            .AllowAnyMethod().AllowAnyHeader().WithOrigins(builder.Configuration["Frontend"])));

        #endregion

        #region Auth

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Auth0:Authority"];
            options.Audience = builder.Configuration["Auth0:Audience"];
        });

        #endregion
    }
    public static void ConfigureWebApplication(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}
