using System;

namespace Symbio.API.Models
{
    public class DeliveryLogEntry
    {
        public int Id { get; set; }
        public int DeliveryAssignmentId { get; set; }
        public string ExpertEmail { get; set; } = string.Empty;
        public string CreatedByEmail { get; set; } = string.Empty;
        public string Level { get; set; } = "info";
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}