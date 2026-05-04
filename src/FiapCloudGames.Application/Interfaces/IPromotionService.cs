using FiapCloudGames.Application.DTOs.Promotions;

namespace FiapCloudGames.Application.Interfaces;

public interface IPromotionService
{
    Task<PromotionResponse> CreateAsync(PromotionRequest request, CancellationToken ct = default);
    Task<PromotionResponse> UpdateAsync(Guid id, PromotionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PromotionResponse> GetAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PromotionResponse>> ListAsync(CancellationToken ct = default);
}
