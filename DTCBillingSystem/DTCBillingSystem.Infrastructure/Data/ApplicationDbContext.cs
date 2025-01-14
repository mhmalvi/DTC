using DTCBillingSystem.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<BillingRate> BillingRates { get; set; }
        public DbSet<MonthlyBill> MonthlyBills { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
} 