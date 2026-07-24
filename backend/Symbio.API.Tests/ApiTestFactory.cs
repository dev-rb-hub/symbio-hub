using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Symbio.API.Data;
using Symbio.API.Models;
using Symbio.Core.Models;

namespace Symbio.API.Tests;

public class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"symbio-api-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.RemoveAll<DbContextOptions<SymbioDbContext>>();
            services.AddDbContext<SymbioDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ =>
            {
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SymbioDbContext>();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                db.Users.AddRange(
                    new User
                    {
                        Email = "expert@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Expert",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "admin@example.com",
                        PasswordHash = SeedData.HashPassword("password123"),
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        OnboardingCompleted = true,
                        OnboardedAt = DateTime.UtcNow
                    });
            }

            if (!db.DeliveryAssignments.Any())
            {
                db.DeliveryAssignments.Add(new DeliveryAssignment
                {
                    Id = 1,
                    ExpertEmail = "expert@example.com",
                    ProjectTitle = "Regional Retail Website Refresh",
                    ClientName = "Harper Bright",
                    Category = "Web Experience",
                    ScopeSummary = "Milestone delivery",
                    CurrentMilestone = "Checkout prototype",
                    Status = "In Progress",
                    ProgressPercent = 48,
                    Priority = "High",
                    DueDate = DateTime.UtcNow.AddDays(7),
                    IsActive = true,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (!db.EscrowOnboardingProfiles.Any())
            {
                db.EscrowOnboardingProfiles.Add(new EscrowOnboardingProfile
                {
                    ExpertEmail = "expert@example.com",
                    ProviderAccountId = "pinch-glassbox-expert-example-com",
                    Status = EscrowOnboardingStatus.Pending.ToString(),
                    OnboardingUrl = "https://connect.getpinch.com.au/glassbox/onboarding/pinch-glassbox-expert-example-com",
                    LastSyncedAtUtc = DateTime.UtcNow
                });
            }

            if (!db.ProjectPaymentStateRecords.Any())
            {
                db.ProjectPaymentStateRecords.Add(new ProjectPaymentStateRecord
                {
                    ProjectId = "demo-project-epic7-1",
                    State = "AwaitingPayment",
                    GrossAmount = 9500m,
                    PlatformFeeAmount = 0m,
                    ContractorAmount = 0m,
                    Currency = "AUD",
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }

            db.SaveChanges();
        });
    }
}
