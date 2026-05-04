namespace FiapCloudGames.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string action, Guid? userId, string description, object? metadata = null, CancellationToken ct = default);
}
