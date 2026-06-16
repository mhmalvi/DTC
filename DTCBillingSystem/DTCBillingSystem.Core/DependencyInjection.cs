using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Shared.Interfaces;

namespace DTCBillingSystem.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Register core services
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IPrintService, PrintService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISMSService, SMSService>();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            return services;
        }
    }
} 