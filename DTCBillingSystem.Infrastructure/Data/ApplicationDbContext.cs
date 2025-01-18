using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Customers = Set<Customer>();
            MonthlyBills = Set<MonthlyBill>();
            PaymentRecords = Set<PaymentRecord>();
            Users = Set<User>();
            MeterReadings = Set<MeterReading>();
            PrintJobs = Set<PrintJob>();
            AuditLogs = Set<AuditLog>();
            BackupInfos = Set<BackupInfo>();
            BackupSchedules = Set<BackupSchedule>();
        }

        public required DbSet<Customer> Customers { get; set; }
        public required DbSet<MonthlyBill> MonthlyBills { get; set; }
        public required DbSet<PaymentRecord> PaymentRecords { get; set; }
        public required DbSet<User> Users { get; set; }
        public required DbSet<MeterReading> MeterReadings { get; set; }
        public required DbSet<PrintJob> PrintJobs { get; set; }
        public required DbSet<AuditLog> AuditLogs { get; set; }
        public required DbSet<BackupInfo> BackupInfos { get; set; }
        public required DbSet<BackupSchedule> BackupSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
} 