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
    }
}
