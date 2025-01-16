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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Create a password hasher instance
            var passwordHasher = new PasswordHasher();

            // Hash the default admin password using PBKDF2
            var password = "Admin@123";
            var (hash, salt) = passwordHasher.HashPassword(password);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@dtcbilling.com",
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                CreatedBy = "system",
                LastModifiedBy = "system",
                RequirePasswordChange = false
            });
        }
    }
} 