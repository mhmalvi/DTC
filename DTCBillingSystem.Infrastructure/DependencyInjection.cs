using System;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using DTCBillingSystem.Infrastructure.Services;

namespace DTCBillingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IMonthlyBillRepository, MonthlyBillRepository>();
            services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
            services.AddScoped<IPrintJobRepository, PrintJobRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IBackupInfoRepository, BackupInfoRepository>();
            services.AddScoped<IBackupScheduleRepository, BackupScheduleRepository>();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, Data.UnitOfWork>();

            // Register services
            services.AddScoped<ICustomerService, DTCBillingSystem.Infrastructure.Services.CustomerService>();
            services.AddScoped<IBillingService, DTCBillingSystem.Infrastructure.Services.BillingService>();
            services.AddScoped<IPaymentService, DTCBillingSystem.Infrastructure.Services.PaymentService>();
            services.AddScoped<IUserService, DTCBillingSystem.Core.Services.UserService>();
            services.AddScoped<IAuditService, DTCBillingSystem.Core.Services.AuditService>();
            services.AddScoped<IBackupService, DTCBillingSystem.Core.Services.BackupService>();
            services.AddScoped<IMeterReadingService, DTCBillingSystem.Core.Services.MeterReadingService>();
            services.AddScoped<IPrintService, DTCBillingSystem.Core.Services.PrintService>();
            services.AddScoped<IReportService, DTCBillingSystem.Core.Services.ReportService>();

            return services;
        }
    }
} 