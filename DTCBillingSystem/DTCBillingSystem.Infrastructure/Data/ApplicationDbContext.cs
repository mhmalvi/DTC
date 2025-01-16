using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;

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
        public DbSet<PrintJob> PrintJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Seed default admin user
            var salt = new byte[32];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var password = "Admin@123";
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
            var combinedBytes = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combinedBytes, passwordBytes.Length, salt.Length);

            var hash = new byte[32];
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                hash = sha256.ComputeHash(combinedBytes);
            }

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