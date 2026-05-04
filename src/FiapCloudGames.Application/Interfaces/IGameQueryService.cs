using FiapCloudGames.Application.DTOs.Games;
using FiapCloudGames.Application.DTOs.Library;

namespace FiapCloudGames.Application.Interfaces;

public interface IGameQueryService
{
    Task<IReadOnlyList<GameResponse>> ListActiveGamesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserGameResponse>> ListUserLibraryAsync(Guid userId, CancellationToken ct = default);
}
