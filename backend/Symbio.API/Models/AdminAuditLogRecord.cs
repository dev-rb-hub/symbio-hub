namespace Symbio.API.Models;

public class AdminAuditLogRecord
{
    public int Id { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string TargetReference { get; set; } = string.Empty;
    public string DetailJson { get; set; } = "{}";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
