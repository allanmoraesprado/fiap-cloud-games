namespace FiapCloudGames.Application.DTOs.Library;

public record UserGameResponse(Guid GameId, string Title, string Genre, decimal Price, DateTime AcquiredAt);
