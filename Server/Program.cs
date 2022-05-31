using DataAccess;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureWebApplicationBuilder();

var app = builder.Build();
app.ConfigureWebApplication();

QueriesMapping.ConfigureQueriesMapping();

app.Run();
