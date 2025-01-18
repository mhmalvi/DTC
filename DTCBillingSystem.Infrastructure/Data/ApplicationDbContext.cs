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
        private readonly IPasswordHasher _passwordHasher;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPasswordHasher passwordHasher)
            : base(options)
        {
            _passwordHasher = passwordHasher;
            
            // Initialize DbSet properties
            Customers = Set<Customer>();
            BillingRates = Set<BillingRate>();
            MonthlyBills = Set<MonthlyBill>();
            PaymentRecords = Set<PaymentRecord>();
            Users = Set<User>();
            AuditLogs = Set<AuditLog>();
            MeterReadings = Set<MeterReading>();
            PrintJobs = Set<PrintJob>();
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<BillingRate> BillingRates { get; set; }
        public DbSet<MonthlyBill> MonthlyBills { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply entity configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure cascade delete behavior for related entities
            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MeterReading>()
                .HasOne(m => m.Customer)
                .WithMany()
                .HasForeignKey(m => m.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fix MonthlyBill relationship
            modelBuilder.Entity<MonthlyBill>(entity =>
            {
                entity.HasOne(b => b.Customer)
                    .WithMany(c => c.MonthlyBills)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(b => b.CustomerId)
                    .IsRequired();
            });

            modelBuilder.Entity<PaymentRecord>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.AccountNumber)
                .IsUnique();
        }
    }
} 