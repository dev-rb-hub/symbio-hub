namespace Symbio.API.Models
{
    public class ProjectPaymentStateRecord
    {
        public int Id { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string State { get; set; } = "AwaitingPayment";
        public decimal GrossAmount { get; set; }
        public decimal PlatformFeeAmount { get; set; }
        public decimal ContractorAmount { get; set; }
        public string Currency { get; set; } = "AUD";
        public string? LastProviderReference { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}