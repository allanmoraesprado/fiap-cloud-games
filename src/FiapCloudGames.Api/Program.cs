using FiapCloudGames.Api.Configuration;
using FiapCloudGames.Api.Middleware;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Infrastructure.Persistence;
using Serilog;

// Keeps Npgsql DateTime handling compatible with the current domain model.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddFcgServices(builder.Configuration);
builder.Services.AddFcgAuth(builder.Configuration);
builder.Services.AddFcgSwagger();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<FcgDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    try { await DbSeeder.SeedAsync(ctx, hasher); }
    catch (Exception ex) { Log.Warning(ex, "Database seeding failed (database may be unavailable)."); }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
