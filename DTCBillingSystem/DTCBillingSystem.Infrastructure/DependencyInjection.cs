using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;

namespace DTCBillingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBillRepository, BillRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            services.AddScoped<INotificationHistoryRepository, NotificationHistoryRepository>();
            services.AddScoped<INotificationSettingsRepository, NotificationSettingsRepository>();
            services.AddScoped<INotificationMessageRepository, NotificationMessageRepository>();
            services.AddScoped<IBackupInfoRepository, BackupInfoRepository>();
            services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IBillingRateRepository, BillingRateRepository>();
            services.AddScoped<IPrintJobRepository, PrintJobRepository>();

            // Register DbContext
            services.AddScoped<ApplicationDbContext>();

            // Register unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
} 