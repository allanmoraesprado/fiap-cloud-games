using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Repositories;
using FiapCloudGames.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Infrastructure.Repositories;

public class UserGameRepository : IUserGameRepository
{
    private readonly FcgDbContext _db;
    public UserGameRepository(FcgDbContext db) => _db = db;

    public Task<UserGame?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default)
        => _db.UserGames.FirstOrDefaultAsync(x => x.UserId == userId && x.GameId == gameId, ct);

    public async Task<IReadOnlyList<UserGame>> ListByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserGames.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task AddAsync(UserGame userGame, CancellationToken ct = default)
        => await _db.UserGames.AddAsync(userGame, ct);
}
