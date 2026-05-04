namespace FiapCloudGames.Application.DTOs.Auth;

public record LoginResponse(string Token, DateTime ExpiresAt, string Email, string Role);
