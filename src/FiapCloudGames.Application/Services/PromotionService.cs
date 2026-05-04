using FiapCloudGames.Application.DTOs.Promotions;
using FiapCloudGames.Application.Interfaces;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Exceptions;
using FiapCloudGames.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FiapCloudGames.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotions;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLogger _audit;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(
        IPromotionRepository promotions,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IAuditLogger audit,
        ILogger<PromotionService> logger)
    {
        _promotions = promotions;
        _uow = uow;
        _currentUser = currentUser;
        _audit = audit;
        _logger = logger;
    }

    public async Task<PromotionResponse> CreateAsync(PromotionRequest request, CancellationToken ct = default)
    {
        EnsureAdmin();
        var promo = new Promotion(request.Title, request.Description, request.DiscountPercentage, request.StartDate, request.EndDate);
        await _promotions.AddAsync(promo, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Promotion created: {Title}", promo.Title);
        await _audit.LogAsync("PromotionCreated", _currentUser.UserId, $"Promotion {promo.Title} created.", new { promo.Id }, ct);
        return Map(promo);
    }

    public async Task<PromotionResponse> UpdateAsync(Guid id, PromotionRequest request, CancellationToken ct = default)
    {
        EnsureAdmin();
        var promo = await _promotions.GetByIdAsync(id, ct) ?? throw new NotFoundException("Promotion not found.");
        promo.Update(request.Title, request.Description, request.DiscountPercentage, request.StartDate, request.EndDate, true);
        await _promotions.UpdateAsync(promo, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(promo);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        EnsureAdmin();
        var promo = await _promotions.GetByIdAsync(id, ct) ?? throw new NotFoundException("Promotion not found.");
        promo.Deactivate();
        await _promotions.UpdateAsync(promo, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PromotionResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        var promo = await _promotions.GetByIdAsync(id, ct) ?? throw new NotFoundException("Promotion not found.");
        return Map(promo);
    }

    public async Task<IReadOnlyList<PromotionResponse>> ListAsync(CancellationToken ct = default)
    {
        var list = await _promotions.ListAsync(ct);
        return list.Select(Map).ToList();
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only Admin can perform this operation.");
    }

    private static PromotionResponse Map(Promotion p) =>
        new(p.Id, p.Title, p.Description, p.DiscountPercentage, p.StartDate, p.EndDate, p.IsActive);
}
