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

            // Seed initial admin user with properly hashed password
            var salt = RandomNumberGenerator.GetBytes(16); // Same size as PasswordHasher.SaltSize
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                "admin123", // Initial password
                salt,
                350000, // Same as PasswordHasher.Iterations
                HashAlgorithmName.SHA256,
                32); // Same as PasswordHasher.HashSize

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
                RequirePasswordChange = true
            });
        }
    }
} 