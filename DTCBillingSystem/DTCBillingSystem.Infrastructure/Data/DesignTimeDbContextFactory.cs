using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dtcbilling.db");

            // Create a mock IPasswordHasher for design-time
            var mockPasswordHasher = new MockPasswordHasher();

            return new ApplicationDbContext(optionsBuilder.Options, mockPasswordHasher);
        }
    }

    // Mock implementation of IPasswordHasher for design-time
    internal class MockPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return "mock_hashed_password";
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return true;
        }
    }
} 