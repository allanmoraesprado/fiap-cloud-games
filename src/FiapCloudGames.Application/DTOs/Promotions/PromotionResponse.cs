namespace FiapCloudGames.Application.DTOs.Promotions;

public record PromotionResponse(Guid Id, string Title, string Description, int DiscountPercentage, DateTime StartDate, DateTime EndDate, bool IsActive);
