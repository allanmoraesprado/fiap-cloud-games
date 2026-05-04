using FiapCloudGames.Application.DTOs.Library;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Exceptions;
using FiapCloudGames.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services;

public class LibraryService : ILibraryService
{
    private readonly IUserGameRepository _userGames;
    private readonly IGameRepository _games;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;
    private readonly IGameQueryService _gameQueries;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(
        IUserGameRepository userGames,
        IGameRepository games,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IAuditLogger audit,
        IGameQueryService gameQueries,
        ILogger<LibraryService> logger)
    {
        _userGames = userGames;
        _games = games;
        _uow = uow;
        _currentUser = currentUser;
        _audit = audit;
        _gameQueries = gameQueries;
        _logger = logger;
    }

    public async Task AcquireAsync(Guid gameId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedException("User must be authenticated.");

        var userId = _currentUser.UserId.Value;
        var game = await _games.GetByIdAsync(gameId, ct) ?? throw new NotFoundException("Game not found.");
        if (!game.IsActive)
            throw new DomainException("Game is not active.");

        var existing = await _userGames.GetAsync(userId, gameId, ct);
        if (existing is not null)
            throw new ConflictException("Game already in user library.");

        var userGame = new UserGame(userId, gameId);
        await _userGames.AddAsync(userGame, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} acquired game {GameId}", userId, gameId);
        await _audit.LogAsync("GameAcquired", userId, $"User acquired game {game.Title}.", new { userId, gameId }, ct);
    }

    public async Task<IReadOnlyList<UserGameResponse>> GetMyLibraryAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedException("User must be authenticated.");
        return await _gameQueries.ListUserLibraryAsync(_currentUser.UserId.Value, ct);
    }

    public async Task<IReadOnlyList<UserGameResponse>> GetUserLibraryAsync(Guid userId, CancellationToken ct = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only Admin can view another user's library.");
        return await _gameQueries.ListUserLibraryAsync(userId, ct);
    }
}
