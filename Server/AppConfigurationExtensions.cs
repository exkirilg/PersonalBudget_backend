using DbUp;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Server;

public static class AppConfigurationExtensions
{
    public static void ConfigureWebApplicationBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

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

        builder.Services.AddDbContext<IdentityContext>(options =>
            options.UseNpgsql(builder.Configuration["ConnectionStrings:PersonalBudgetIdentityConnection"]));
        
        builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
            options.User.RequireUniqueEmail = true;
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:Audience"],
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Auth:JWTSecret"])),
                ValidateIssuerSigningKey = true
            };
        });

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(AuthorizationPolicies.MustBeOperationAuthor, policy =>
                policy.Requirements.Add(new MustBeOperationAuthorRequirement())
            )
        );

        builder.Services.AddScoped<IAuthorizationHandler, MustBeOperationAuthorHandler>();

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
