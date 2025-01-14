using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DTCBillingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<DbContext>(provider => provider.GetService<ApplicationDbContext>());

            services.AddScoped<IRepository<Customer>, Repository<Customer>>();
            services.AddScoped<IRepository<MonthlyBill>, Repository<MonthlyBill>>();
            services.AddScoped<IRepository<PaymentRecord>, Repository<PaymentRecord>>();
            services.AddScoped<IRepository<MeterReading>, Repository<MeterReading>>();
            services.AddScoped<IRepository<NotificationMessage>, Repository<NotificationMessage>>();
            services.AddScoped<IRepository<BackupInfo>, Repository<BackupInfo>>();
            services.AddScoped<IRepository<BackupSchedule>, Repository<BackupSchedule>>();
            services.AddScoped<IRepository<User>, Repository<User>>();
            services.AddScoped<IRepository<AuditLog>, Repository<AuditLog>>();
            services.AddScoped<IRepository<BillingRate>, Repository<BillingRate>>();
            services.AddScoped<IRepository<PrintJob>, Repository<PrintJob>>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
} 