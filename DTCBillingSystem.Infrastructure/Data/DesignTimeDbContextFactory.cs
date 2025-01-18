using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? Path.Combine(Directory.GetCurrentDirectory(), "dtcbilling.db");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options)
            {
                Customers = Set<Customer>(),
                MonthlyBills = Set<MonthlyBill>(),
                PaymentRecords = Set<PaymentRecord>(),
                Users = Set<User>(),
                MeterReadings = Set<MeterReading>(),
                PrintJobs = Set<PrintJob>(),
                AuditLogs = Set<AuditLog>(),
                BackupInfos = Set<BackupInfo>(),
                BackupSchedules = Set<BackupSchedule>()
            };
        }

        private DbSet<T> Set<T>() where T : class => null!;
    }
} 