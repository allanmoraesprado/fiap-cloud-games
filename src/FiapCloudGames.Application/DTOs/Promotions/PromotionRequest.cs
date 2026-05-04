namespace FiapCloudGames.Application.DTOs.Promotions;

public record PromotionRequest(string Title, string Description, int DiscountPercentage, DateTime StartDate, DateTime EndDate);
