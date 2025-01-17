using System;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task SeedAsync()
        {
            try
            {
                Debug.WriteLine("Starting database seeding process...");

                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();
                Debug.WriteLine("Database created successfully");

                // Seed users first
                await SeedUsersWithTransactionAsync();
                Debug.WriteLine("Users seeded successfully");

                // Check if we need to seed other data
                if (!await _context.Customers.AnyAsync())
                {
                    Debug.WriteLine("No existing customers found, seeding initial data...");
                    await SeedCustomersAsync();
                    Debug.WriteLine("Customers seeded successfully");
                }

                Debug.WriteLine("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SeedAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner error: {ex.InnerException.Message}");
                    Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                throw;
            }
        }

        private async Task SeedUsersWithTransactionAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if admin user already exists
                if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
                {
                    Debug.WriteLine("Admin user does not exist, creating...");

                    // Create admin user with proper hash and salt handling
                    var (hashBytes, saltBytes) = _passwordHasher.HashPassword("admin123");
                    Debug.WriteLine($"Generated hash length: {hashBytes?.Length ?? 0}, salt length: {saltBytes?.Length ?? 0}");

                    if (hashBytes == null || saltBytes == null)
                    {
                        throw new Exception("Password hashing failed - hash or salt is null");
                    }

                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@dtcbilling.com",
                        FirstName = "System",
                        LastName = "Administrator",
                        PasswordHash = hashBytes,
                        PasswordSalt = saltBytes,
                        Role = UserRole.Administrator,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        LastModifiedAt = DateTime.UtcNow,
                        CreatedBy = "system",
                        LastModifiedBy = "system",
                        RequirePasswordChange = false
                    };

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Admin user added successfully");
                }
                else
                {
                    Debug.WriteLine("Admin user already exists, skipping creation");
                }

                await transaction.CommitAsync();
                Debug.WriteLine("User seeding transaction committed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"Error seeding users: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private async Task SeedCustomersAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add sample customers if none exist
                if (!await _context.Customers.AnyAsync())
                {
                    var customer = new Customer
                    {
                        AccountNumber = "DTC001",
                        Name = "John Doe",
                        Address = "123 Main St",
                        ContactNumber = "1234567890",
                        PhoneNumber = "1234567890",
                        Email = "john.doe@example.com",
                        CustomerType = CustomerType.Regular,
                        ZoneCode = "Z001",
                        MeterNumber = "M001",
                        ShopNo = "S001",
                        Floor = "1st",
                        RegistrationDate = DateTime.UtcNow,
                        ConnectionDate = DateTime.UtcNow,
                        Status = CustomerStatus.Active,
                        IsActive = true,
                        Notes = "Sample customer",
                        SecurityDeposit = 1000,
                        CurrentBalance = 0,
                        LastBillingDate = DateTime.UtcNow,
                        LastPaymentDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        LastModifiedAt = DateTime.UtcNow,
                        CreatedBy = "system",
                        LastModifiedBy = "system"
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                Debug.WriteLine("Customer seeding completed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Debug.WriteLine($"Error seeding customers: {ex.Message}");
                throw;
            }
        }
    }
} 