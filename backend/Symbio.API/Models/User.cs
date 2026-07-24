namespace Symbio.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public string CompanyName { get; set; } = string.Empty;
        public string BusinessIdentifier { get; set; } = string.Empty;
        public string ProfileSummary { get; set; } = string.Empty;
        public bool OnboardingCompleted { get; set; }
        public DateTime? OnboardedAt { get; set; }
    }
}
