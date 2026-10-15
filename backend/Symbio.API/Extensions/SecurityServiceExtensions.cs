using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Symbio.API.Services;

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

            var hasSigningSecret = !string.IsNullOrWhiteSpace(secret);
            SymmetricSecurityKey? key = null;

            if (hasSigningSecret)
            {
                key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
            }

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
                    ValidateIssuerSigningKey = hasSigningSecret,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = System.TimeSpan.FromMinutes(5)
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireSmeRole", policy => policy.RequireRole("SME"));
                options.AddPolicy("RequireExpertRole", policy => policy.RequireRole("Expert"));
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin").RequireClaim("symbio_admin_master", "true"));
            });

            // OWASP guidance for PBKDF2-HMAC-SHA512 is 210,000 iterations; the floor stops a misconfiguration weakening it.
            var iterations = Math.Max(
                MinimumPasswordIterations,
                configuration.GetValue("Security:PasswordHashing:IterationCount", DefaultPasswordIterations));
            services.Configure<PasswordHasherOptions>(options => options.IterationCount = iterations);
            services.AddSingleton<IPasswordService, PasswordService>();

            return services;
        }

        private const int DefaultPasswordIterations = 210_000;
        private const int MinimumPasswordIterations = 100_000;
    }
}
