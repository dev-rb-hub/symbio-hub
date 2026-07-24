using System;

namespace Symbio.API.Models
{
    public class DeliveryAssignment
    {
        public int Id { get; set; }
        public string ExpertEmail { get; set; } = string.Empty;
        public string ProjectTitle { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ScopeSummary { get; set; } = string.Empty;
        public string CurrentMilestone { get; set; } = string.Empty;
        public string Status { get; set; } = "In Progress";
        public int ProgressPercent { get; set; }
        public string Priority { get; set; } = "Medium";
        public DateTime DueDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}