using Microsoft.EntityFrameworkCore;
using Symbio.API.Models;

namespace Symbio.API.Data
{
    public static class SeedData
    {
        public static void Initialize(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            db.Database.EnsureCreated();

            if (db.Jobs.Any())
            {
                return;
            }

            var jobs = new[]
            {
                new Job
                {
                    Title = "Regional Retail Website Refresh",
                    Description = "Build a mobile-first homepage and checkout experience for a small NSW retail brand.",
                    ClientName = "Harper",
                    ClientSurname = "Bright",
                    Budget = 9500m,
                    ContactEmail = "contact@harperbright.com",
                    IsPublished = true,
                    PostedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Job
                {
                    Title = "Local Healthcare Data Dashboard",
                    Description = "Create a lightweight reporting dashboard for a regional practice using anonymised patient metrics.",
                    ClientName = "Jade",
                    ClientSurname = "Taylor",
                    Budget = 14500m,
                    ContactEmail = "jade.taylor@coastalhealth.au",
                    IsPublished = true,
                    PostedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Job
                {
                    Title = "Food Delivery Loyalty Campaign",
                    Description = "Design and build a customer loyalty landing page with signup flow and campaign analytics.",
                    ClientName = "Miles",
                    ClientSurname = "Kerr",
                    Budget = 7200m,
                    ContactEmail = "miles@harvestdeli.au",
                    IsPublished = true,
                    PostedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            db.Jobs.AddRange(jobs);
            db.SaveChanges();
        }
    }
}
