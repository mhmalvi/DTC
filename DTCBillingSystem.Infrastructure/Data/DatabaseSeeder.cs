using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private const int SYSTEM_USER_ID = 1;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
                
                if (!await _context.Users.AnyAsync())
                {
                    Debug.WriteLine("Starting to seed users...");
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        Email = "admin@dtc.com",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastModifiedAt = DateTime.UtcNow,
                        CreatedBy = SYSTEM_USER_ID,
                        LastModifiedBy = SYSTEM_USER_ID
                    };

                    await _context.Users.AddAsync(adminUser);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Successfully added admin user");
                }

                if (!await _context.Customers.AnyAsync())
                {
                    Debug.WriteLine("Starting to seed customers...");
                    var customers = new[]
                    {
                        new Customer
                        {
                            AccountNumber = "CUST001",
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@example.com",
                            PhoneNumber = "1234567890",
                            Address = "123 Main St",
                            IsActive = true,
                            CustomerType = CustomerType.Regular,
                            Status = CustomerStatus.Active,
                            RegistrationDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            LastModifiedAt = DateTime.UtcNow,
                            CreatedBy = SYSTEM_USER_ID,
                            LastModifiedBy = SYSTEM_USER_ID
                        },
                        new Customer
                        {
                            AccountNumber = "CUST002",
                            FirstName = "Jane",
                            LastName = "Smith",
                            Email = "jane.smith@example.com",
                            PhoneNumber = "0987654321",
                            Address = "456 Oak Ave",
                            IsActive = true,
                            CustomerType = CustomerType.Regular,
                            Status = CustomerStatus.Active,
                            RegistrationDate = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            LastModifiedAt = DateTime.UtcNow,
                            CreatedBy = SYSTEM_USER_ID,
                            LastModifiedBy = SYSTEM_USER_ID
                        }
                    };

                    await _context.Customers.AddRangeAsync(customers);
                    var result = await _context.SaveChangesAsync();
                    Debug.WriteLine($"Successfully added {result} customers to database");
                }
                else
                {
                    var count = await _context.Customers.CountAsync();
                    Debug.WriteLine($"Database already contains {count} customers");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error seeding database: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }
} 