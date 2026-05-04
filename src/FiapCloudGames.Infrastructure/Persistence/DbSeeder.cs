using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(FcgDbContext context, IPasswordHasher hasher, CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        if (!await context.Users.AnyAsync(ct))
        {
            var admin = new User("Administrator", "admin@fcg.com", hasher.Hash("Admin@123"), UserRole.Admin);
            var user = new User("Regular User", "user@fcg.com", hasher.Hash("User@123"), UserRole.User);
            await context.Users.AddRangeAsync(new[] { admin, user }, ct);
        }

        if (!await context.Games.AnyAsync(ct))
        {
            var games = new[]
            {
                new Game("Cosmic Odyssey", "Sci-fi RPG set across galaxies.", "RPG", 199.90m, new DateTime(2024, 03, 15)),
                new Game("Shadow Realms", "Action adventure with dark fantasy.", "Action", 149.90m, new DateTime(2023, 11, 02)),
                new Game("Pixel Racers", "Retro-style arcade racing.", "Racing", 49.90m, new DateTime(2025, 01, 20)),
                new Game("Mythic Lands", "Open-world fantasy MMO.", "MMORPG", 99.90m, new DateTime(2022, 06, 10))
            };
            await context.Games.AddRangeAsync(games, ct);
        }

        await context.SaveChangesAsync(ct);
    }
}
