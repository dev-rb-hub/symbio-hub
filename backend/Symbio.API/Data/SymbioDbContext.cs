using Microsoft.EntityFrameworkCore;
using Symbio.API.Models;

namespace Symbio.API.Data
{
    public class SymbioDbContext : DbContext
    {
        public SymbioDbContext(DbContextOptions<SymbioDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<DeliveryAssignment> DeliveryAssignments { get; set; } = null!;
        public DbSet<DeliveryLogEntry> DeliveryLogs { get; set; } = null!;
        public DbSet<EscrowOnboardingProfile> EscrowOnboardingProfiles { get; set; } = null!;
        public DbSet<ProjectPaymentStateRecord> ProjectPaymentStateRecords { get; set; } = null!;
        public DbSet<PaymentPreApprovalRecord> PaymentPreApprovals { get; set; } = null!;
        public DbSet<DirectDebitPullRequestRecord> DirectDebitPullRequests { get; set; } = null!;
        public DbSet<AccountingInvoiceRecord> AccountingInvoices { get; set; } = null!;
        public DbSet<RetainerContractRecord> RetainerContracts { get; set; } = null!;
        public DbSet<RetainerUsageRecord> RetainerUsages { get; set; } = null!;
        public DbSet<RetainerChargeRecord> RetainerCharges { get; set; } = null!;
        public DbSet<AdminProjectFlagRecord> AdminProjectFlagRecords { get; set; } = null!;
        public DbSet<AdminUserComplianceRecord> AdminUserComplianceRecords { get; set; } = null!;
        public DbSet<AdminSafetySettingRecord> AdminSafetySettings { get; set; } = null!;
        public DbSet<AdminAuditLogRecord> AdminAuditLogs { get; set; } = null!;
    }
}
