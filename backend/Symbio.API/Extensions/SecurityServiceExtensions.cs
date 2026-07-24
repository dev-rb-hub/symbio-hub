using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Symbio.API.Extensions
{
    public static class SecurityServiceExtensions
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var secret = jwtSection["Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? string.Empty));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = !string.IsNullOrEmpty(secret),
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = System.TimeSpan.FromMinutes(5)
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireSmeRole", policy => policy.RequireRole("SME"));
                options.AddPolicy("RequireExpertRole", policy => policy.RequireRole("Expert"));
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            });

            return services;
        }
    }
}
