using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Repositories;
using FiapCloudGames.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FiapCloudGames.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly FcgDbContext _db;
    public PromotionRepository(FcgDbContext db) => _db = db;

    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Promotions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Promotion>> ListAsync(CancellationToken ct = default)
        => await _db.Promotions.AsNoTracking().OrderByDescending(p => p.StartDate).ToListAsync(ct);

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default)
        => await _db.Promotions.AddAsync(promotion, ct);

    public Task UpdateAsync(Promotion promotion, CancellationToken ct = default)
    {
        _db.Promotions.Update(promotion);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Promotion promotion, CancellationToken ct = default)
    {
        _db.Promotions.Remove(promotion);
        return Task.CompletedTask;
    }
}
