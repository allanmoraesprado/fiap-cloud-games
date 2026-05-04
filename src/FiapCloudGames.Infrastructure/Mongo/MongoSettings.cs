namespace FiapCloudGames.Infrastructure.Mongo;

public class MongoSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = "fcg_audit";
    public string AuditCollection { get; set; } = "audit_logs";
}
