using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Repositories;

public interface IUserGameRepository
{
    Task<UserGame?> GetAsync(Guid userId, Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<UserGame>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserGame userGame, CancellationToken ct = default);
}
