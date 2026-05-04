namespace FiapCloudGames.Application.DTOs.Games;

public record GameRequest(string Title, string Description, string Genre, decimal Price, DateTime ReleaseDate);
