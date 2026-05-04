using FiapCloudGames.Application.DTOs.Promotions;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Exceptions;
using FiapCloudGames.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FiapCloudGames.Tests.Services;

public class PromotionServiceTests
{
    private readonly Mock<IPromotionRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUser> _user = new();
    private readonly Mock<IAuditLogger> _audit = new();

    private PromotionService Build() => new(_repo.Object, _uow.Object, _user.Object, _audit.Object, NullLogger<PromotionService>.Instance);

    [Fact]
    public async Task NonAdmin_cannot_create()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(false);
        var req = new PromotionRequest("T", "D", 10, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        await ((Func<Task>)(() => Build().CreateAsync(req))).Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Discount_outside_range_is_rejected()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(true);
        var req = new PromotionRequest("T", "D", 0, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        await ((Func<Task>)(() => Build().CreateAsync(req))).Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task EndDate_before_StartDate_is_rejected()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(true);
        var now = DateTime.UtcNow;
        var req = new PromotionRequest("T", "D", 10, now.AddDays(2), now);
        await ((Func<Task>)(() => Build().CreateAsync(req))).Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Admin_can_create_valid_promotion()
    {
        _user.SetupGet(u => u.IsAdmin).Returns(true);
        var req = new PromotionRequest("Promo", "Desc", 25, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        var resp = await Build().CreateAsync(req);
        resp.Title.Should().Be("Promo");
        resp.DiscountPercentage.Should().Be(25);
    }
}
