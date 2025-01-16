using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure TokenService with settings from configuration
            var jwtSettings = configuration.GetSection("JwtSettings");
            services.AddScoped<ITokenService>(provider => new TokenService(
                jwtSettings["SecretKey"],
                jwtSettings["Issuer"],
                jwtSettings["Audience"],
                int.Parse(jwtSettings["ExpirationMinutes"] ?? "60")
            ));

            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
} 