using FiapCloudGames.Application.Validators;
using FluentAssertions;
using Xunit;

namespace FiapCloudGames.Tests.Validators;

public class PromotionValidatorTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void Discount_must_be_between_1_and_100(int discount, bool expected)
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(1);
        PromotionValidator.Validate(discount, start, end).IsValid.Should().Be(expected);
    }

    [Fact]
    public void EndDate_must_be_greater_than_StartDate()
    {
        var now = DateTime.UtcNow;
        PromotionValidator.Validate(10, now, now).IsValid.Should().BeFalse();
        PromotionValidator.Validate(10, now, now.AddDays(-1)).IsValid.Should().BeFalse();
        PromotionValidator.Validate(10, now, now.AddDays(1)).IsValid.Should().BeTrue();
    }
}
