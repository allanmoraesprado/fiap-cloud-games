using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Exceptions;
using FiapCloudGames.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services;

public class LibraryServiceTests
{
    private readonly Mock<IUserGameRepository> _userGames = new();
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IGameQueryService> _queries = new();

    private LibraryService Build() => new(_userGames.Object, _games.Object, _uow.Object, _user.Object, _audit.Object, _queries.Object, NullLogger<LibraryService>.Instance);

    private void Authenticate(Guid id)
    {
        _user.SetupGet(u => u.IsAuthenticated).Returns(true);
        _user.SetupGet(u => u.UserId).Returns(id);
    }

    [Fact]
    public async Task Acquire_throws_when_unauthenticated()
    {
        _user.SetupGet(u => u.IsAuthenticated).Returns(false);
        var act = () => Build().AcquireAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Acquire_throws_when_game_not_found()
    {
        Authenticate(Guid.NewGuid());
        _games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);
        var act = () => Build().AcquireAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task User_cannot_acquire_same_game_twice()
    {
        var userId = Guid.NewGuid();
        var game = new Game("T", "D", "G", 10m, DateTime.UtcNow);
        Authenticate(userId);
        _games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);
        _userGames.Setup(r => r.GetAsync(userId, game.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new UserGame(userId, game.Id));

        var act = () => Build().AcquireAsync(game.Id);
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Acquire_succeeds_when_first_time()
    {
        var userId = Guid.NewGuid();
        var game = new Game("T", "D", "G", 10m, DateTime.UtcNow);
        Authenticate(userId);
        _games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(game);
        _userGames.Setup(r => r.GetAsync(userId, game.Id, It.IsAny<CancellationToken>())).ReturnsAsync((UserGame?)null);

        await Build().AcquireAsync(game.Id);
        _userGames.Verify(r => r.AddAsync(It.IsAny<UserGame>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
