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
    }
}
