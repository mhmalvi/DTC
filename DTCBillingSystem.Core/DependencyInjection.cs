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
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured");
            if (string.IsNullOrEmpty(issuer))
                throw new InvalidOperationException("JWT Issuer is not configured");
            if (string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Audience is not configured");

            // Register singleton services
            services.AddSingleton<ITokenService>(new TokenService(
                secretKey,
                issuer,
                audience));

            // Register core services in correct dependency order
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
} 