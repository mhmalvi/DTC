using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
} 