namespace Symbio.API.Models
{
    public class EscrowOnboardingProfile
    {
        public int Id { get; set; }
        public string ExpertEmail { get; set; } = string.Empty;
        public string ProviderAccountId { get; set; } = string.Empty;
        public string Status { get; set; } = "NotStarted";
        public string OnboardingUrl { get; set; } = string.Empty;
        public DateTime LastSyncedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? OnboardedAtUtc { get; set; }
    }
}