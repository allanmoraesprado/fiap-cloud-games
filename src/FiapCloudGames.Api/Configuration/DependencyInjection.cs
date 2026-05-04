using System.Text;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Repositories;
using FiapCloudGames.Infrastructure.Dapper;
using FiapCloudGames.Infrastructure.Mongo;
using FiapCloudGames.Infrastructure.Persistence;
using FiapCloudGames.Infrastructure.Repositories;
using FiapCloudGames.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FiapCloudGames.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddFcgServices(this IServiceCollection services, IConfiguration config)
    {
        var pgConn = config.GetConnectionString("Postgres")
                     ?? throw new InvalidOperationException("Missing Postgres connection string.");

        services.AddDbContext<FcgDbContext>(opts => opts.UseNpgsql(pgConn));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FcgDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IUserGameRepository, UserGameRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILibraryService, LibraryService>();
        services.AddScoped<IPromotionService, PromotionService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.Configure<MongoSettings>(config.GetSection("Mongo"));
        services.AddSingleton<IAuditLogger, MongoAuditLogger>();

        services.AddSingleton<IGameQueryService>(_ => new GameQueryService(pgConn));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    public static IServiceCollection AddFcgAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Missing Jwt settings.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("AuthenticatedUser", p => p.RequireAuthenticatedUser());
        });

        return services;
    }
}
