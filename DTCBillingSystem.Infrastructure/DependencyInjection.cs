using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Services;
using DTCBillingSystem.Infrastructure.Repositories;

namespace DTCBillingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            services.AddScoped<IBillingPeriodRepository, BillingPeriodRepository>();
            services.AddScoped<IMeterReadingScheduleRepository, MeterReadingScheduleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IBackupRepository, BackupRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IMonthlyBillRepository, MonthlyBillRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuditService, AuditService>();

            return services;
        }
    }
} 