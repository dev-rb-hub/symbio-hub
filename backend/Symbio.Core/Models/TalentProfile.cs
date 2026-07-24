using System;
using System.Collections.Generic;

namespace Symbio.Core.Models
{
    public class TalentProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Expert";
        public string Location { get; set; } = string.Empty;
        public string ProfileSummary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<string> Services { get; set; } = new();
        public decimal HourlyRate { get; set; }
        public string Availability { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = true;
        public bool IsDiscoverable { get; set; } = true;
        public int FeaturedRank { get; set; }
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    }
}