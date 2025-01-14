using System;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<MonthlyBill> Bills { get; set; }
        public DbSet<PaymentRecord> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BillingRate> BillingRates { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<NotificationHistory> NotificationHistory { get; set; }
        public DbSet<NotificationSettings> NotificationSettings { get; set; }
        public DbSet<NotificationMessage> NotificationMessages { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<BackupInfo> BackupInfo { get; set; }
        public DbSet<BackupSchedule> BackupSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.AccountNumber).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<MonthlyBill>(entity =>
            {
                entity.HasOne(b => b.Customer)
                    .WithMany(c => c.Bills)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PaymentRecord>(entity =>
            {
                entity.HasOne(p => p.Bill)
                    .WithMany(b => b.Payments)
                    .HasForeignKey(p => p.BillId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MeterReading>(entity =>
            {
                entity.HasOne(m => m.Customer)
                    .WithMany(c => c.MeterReadings)
                    .HasForeignKey(m => m.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NotificationHistory>(entity =>
            {
                entity.HasOne(n => n.Customer)
                    .WithMany(c => c.NotificationHistory)
                    .HasForeignKey(n => n.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NotificationSettings>(entity =>
            {
                entity.HasOne(n => n.Customer)
                    .WithOne(c => c.NotificationSettings)
                    .HasForeignKey<NotificationSettings>(n => n.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BillingRate>(entity =>
            {
                entity.HasIndex(e => e.EffectiveFrom);
            });

            modelBuilder.Entity<PrintJob>(entity =>
            {
                entity.HasIndex(e => e.JobId).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<BackupInfo>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Type);
            });

            modelBuilder.Entity<BackupSchedule>(entity =>
            {
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.Frequency);
            });
        }
    }
} 