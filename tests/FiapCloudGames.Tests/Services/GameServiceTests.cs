using FiapCloudGames.Application.DTOs.Games;
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

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<ICurrentUser> _user = new();

    private GameService Build() => new(_games.Object, _uow.Object, _audit.Object, _user.Object, NullLogger<GameService>.Instance);

    [Fact]
    public async Task NonAdmin_cannot_create_game()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(false);
        var act = () => Build().CreateAsync(new GameRequest("T", "D", "G", 10m, DateTime.UtcNow));
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Admin_can_create_game()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(true);
        var resp = await Build().CreateAsync(new GameRequest("Title", "Desc", "Action", 99m, DateTime.UtcNow));
        resp.Title.Should().Be("Title");
        _games.Verify(r => r.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NonAdmin_cannot_delete_game()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(false);
        var act = () => Build().DeleteAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
