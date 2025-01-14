using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Shared.Interfaces;

namespace DTCBillingSystem.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IPrintService, PrintService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
} 