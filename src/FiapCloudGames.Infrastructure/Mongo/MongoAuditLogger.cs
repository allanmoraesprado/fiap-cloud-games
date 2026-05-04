using FiapCloudGames.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FiapCloudGames.Infrastructure.Mongo;

public class MongoAuditLogger : IAuditLogger
{
    private readonly IMongoCollection<AuditLog> _collection;
    private readonly ILogger<MongoAuditLogger> _logger;

    public MongoAuditLogger(IOptions<MongoSettings> options, ILogger<MongoAuditLogger> logger)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var db = client.GetDatabase(settings.Database);
        _collection = db.GetCollection<AuditLog>(settings.AuditCollection);
        _logger = logger;
    }

    public async Task LogAsync(string action, Guid? userId, string description, object? metadata = null, CancellationToken ct = default)
    {
        try
        {
            var log = new AuditLog
            {
                Action = action,
                UserId = userId,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                Metadata = metadata is null ? null : BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(metadata))
            };
            await _collection.InsertOneAsync(log, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write audit log for action {Action}", action);
        }
    }
}
