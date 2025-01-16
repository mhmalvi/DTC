using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public DbInitializer(
            ApplicationDbContext context,
            IConfiguration configuration,
            IPasswordHasher passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.MigrateAsync();

                // Check if admin user exists
                if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
                {
                    var adminPassword = _configuration["AdminSettings:DefaultPassword"] ?? "Admin@123";
                    var hashedPassword = _passwordHasher.HashPassword(adminPassword);

                    var adminUser = new User
                    {
                        Username = "admin",
                        Password = hashedPassword,
                        Email = "admin@dtcbilling.com",
                        FirstName = "System",
                        LastName = "Administrator",
                        Role = UserRole.Administrator,
                        Status = UserStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    await _context.Users.AddAsync(adminUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize database.", ex);
            }
        }
    }
} 