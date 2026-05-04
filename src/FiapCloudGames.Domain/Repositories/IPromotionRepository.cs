using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task UpdateAsync(Promotion promotion, CancellationToken ct = default);
    Task DeleteAsync(Promotion promotion, CancellationToken ct = default);
}
