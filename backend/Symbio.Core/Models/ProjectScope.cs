using System.Collections.Generic;

namespace Symbio.Core.Models
{
    public class ProjectScope
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string ClientEmail { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public string PaymentState { get; set; } = "AwaitingPayment";
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public List<ProjectMilestone> Milestones { get; set; } = new();
    }

    public class ProjectMilestone
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
