using System;
using System.Collections.Generic;

namespace Symbio.Core.Models
{
    public static class TalentSeedProfiles
    {
        public static IReadOnlyList<TalentProfile> DefaultProfiles { get; } = new List<TalentProfile>
        {
            new()
            {
                Name = "Ava Chen",
                CompanyName = "North Coast Digital",
                Email = "ava.chen@example.com",
                Role = "Expert",
                Location = "Newcastle, NSW",
                ProfileSummary = "Delivery-focused full stack engineer with strong experience in compliance-first portals and SME workflow automation.",
                Skills = new List<string> { "React", "TypeScript", ".NET", "Workflow automation" },
                Services = new List<string> { "Product discovery", "Frontend delivery", "API integration" },
                HourlyRate = 165,
                Availability = "Available within 1 week",
                IsVerified = true,
                IsDiscoverable = true,
                FeaturedRank = 95,
                LastActiveAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Name = "Noah Patel",
                CompanyName = "Blue Gum Analytics",
                Email = "noah.patel@example.com",
                Role = "Expert",
                Location = "Wollongong, NSW",
                ProfileSummary = "Data and dashboard specialist helping regional businesses turn operational data into usable reporting tools.",
                Skills = new List<string> { "Power BI", "Data modelling", "Cosmos DB", "Reporting dashboards" },
                Services = new List<string> { "Analytics setup", "Dashboard delivery", "Data quality reviews" },
                HourlyRate = 145,
                Availability = "Available now",
                IsVerified = true,
                IsDiscoverable = true,
                FeaturedRank = 90,
                LastActiveAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Name = "Sophie Nguyen",
                CompanyName = "Rural Cloud Studio",
                Email = "sophie.nguyen@example.com",
                Role = "Expert",
                Location = "Tamworth, NSW",
                ProfileSummary = "Cloud migration and automation consultant focused on practical delivery for smaller teams and local operators.",
                Skills = new List<string> { "Azure", "Automation", "DevOps", "Security review" },
                Services = new List<string> { "Cloud migration", "Automation planning", "Operational hardening" },
                HourlyRate = 180,
                Availability = "2 days per week",
                IsVerified = true,
                IsDiscoverable = true,
                FeaturedRank = 88,
                LastActiveAt = DateTime.UtcNow.AddDays(-2)
            }
        };
    }
}