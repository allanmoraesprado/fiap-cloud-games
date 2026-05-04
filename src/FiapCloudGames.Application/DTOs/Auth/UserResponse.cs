namespace FiapCloudGames.Application.DTOs.Auth;

public record UserResponse(Guid Id, string Name, string Email, string Role, DateTime CreatedAt);
