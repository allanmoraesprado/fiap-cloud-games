using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}
