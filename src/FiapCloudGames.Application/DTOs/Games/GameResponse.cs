namespace FiapCloudGames.Application.DTOs.Games;

public record GameResponse(Guid Id, string Title, string Description, string Genre, decimal Price, DateTime ReleaseDate, bool IsActive);
