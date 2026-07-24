namespace Symbio.API.Models;

public class AdminSafetySettingRecord
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string UpdatedByEmail { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
