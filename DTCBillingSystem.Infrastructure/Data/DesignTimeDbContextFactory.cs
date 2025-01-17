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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                "Data Source=DTCBillingSystem.db";

            optionsBuilder.UseSqlite(connectionString);

            // Create a mock IPasswordHasher for design-time
            var mockPasswordHasher = new MockPasswordHasher();

            return new ApplicationDbContext(optionsBuilder.Options, mockPasswordHasher);
        }
    }

    // Mock implementation of IPasswordHasher for design-time
    internal class MockPasswordHasher : IPasswordHasher
    {
        public (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            var mockHash = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var mockSalt = new byte[] { 0x04, 0x05, 0x06, 0x07 };
            return (mockHash, mockSalt);
        }

        public bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            return true;
        }
    }
} 