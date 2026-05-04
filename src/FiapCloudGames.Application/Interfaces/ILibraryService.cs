using FiapCloudGames.Application.DTOs.Library;

namespace FiapCloudGames.Application.Interfaces;

public interface ILibraryService
{
    Task AcquireAsync(Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<UserGameResponse>> GetMyLibraryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserGameResponse>> GetUserLibraryAsync(Guid userId, CancellationToken ct = default);
}
