namespace Symbio.API.Models;

public class AdminUserComplianceRecord
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string ReviewStatus { get; set; } = "Pending";
    public string RiskLevel { get; set; } = "Low";
    public string Notes { get; set; } = string.Empty;
    public string? ReviewedByEmail { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
}
