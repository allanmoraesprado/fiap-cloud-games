using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FiapCloudGames.Infrastructure.Mongo;

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Action { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public BsonDocument? Metadata { get; set; }
}
