using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Repositories;
using FiapCloudGames.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly FcgDbContext _db;
    public GameRepository(FcgDbContext db) => _db = db;

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Games.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Game>> ListAsync(CancellationToken ct = default)
        => await _db.Games.AsNoTracking().OrderBy(g => g.Title).ToListAsync(ct);

    public async Task AddAsync(Game game, CancellationToken ct = default)
        => await _db.Games.AddAsync(game, ct);

    public Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        _db.Games.Update(game);
        return Task.CompletedTask;
    }
}
