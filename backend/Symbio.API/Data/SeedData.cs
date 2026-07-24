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

            if (!db.Users.Any())
            {
                var users = new[]
                {
                    new User
                    {
                        Email = "sme@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "SME",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "Coastal SME Services",
                        BusinessIdentifier = "ABN 12 345 678 901",
                        ProfileSummary = "Regional digital transformation for small businesses.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "expert@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Expert",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "North Shore Dev Studio",
                        BusinessIdentifier = "ABN 98 765 432 109",
                        ProfileSummary = "Freelance expert in compliance-first application delivery.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "admin@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        CompanyName = "Symbio Platform Admin",
                        BusinessIdentifier = "ABN 00 000 000 000",
                        ProfileSummary = "Platform administrator with full system oversight.",
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    }
                };

                db.Users.AddRange(users);
                db.SaveChanges();
            }

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

        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password ?? string.Empty);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
