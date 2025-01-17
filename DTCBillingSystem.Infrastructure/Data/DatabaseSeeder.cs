using System;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync()
        {
            // Check if there are any admin users
            var hasAdminUser = await _context.Users
                .AnyAsync(u => u.Role == UserRole.Administrator);

            // Only seed users if there are no admin users
            if (!hasAdminUser)
            {
                await SeedUsersAsync();
            }
            
            // Only seed other data if there are no customers
            if (!await _context.Customers.AnyAsync())
            {
                await SeedCustomersAsync();
                await SeedBillingRatesAsync();
                await SeedMonthlyBillsAsync();
                await SeedPaymentRecordsAsync();
            }
            
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            var (hash, salt) = _passwordHasher.HashPassword("admin123");

            var adminUser = new User
            {
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
            };

            await _context.Users.AddAsync(adminUser);
        }

        private async Task SeedCustomersAsync()
        {
            var customers = new[]
            {
                new Customer
                {
                    AccountNumber = "DTC001",
                    Name = "John Doe",
                    Address = "123 Main St, City",
                    ContactNumber = "9876543210",
                    Email = "john.doe@example.com",
                    CustomerType = CustomerType.Regular,
                    ConnectionDate = DateTime.Now.AddYears(-2),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                },
                new Customer
                {
                    AccountNumber = "DTC002",
                    Name = "Jane Smith",
                    Address = "456 Oak St, City",
                    ContactNumber = "9876543211",
                    Email = "jane.smith@example.com",
                    CustomerType = CustomerType.Premium,
                    ConnectionDate = DateTime.Now.AddYears(-1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                },
                new Customer
                {
                    AccountNumber = "DTC003",
                    Name = "Bob Wilson",
                    Address = "789 Pine St, City",
                    ContactNumber = "9876543212",
                    Email = "bob.wilson@example.com",
                    CustomerType = CustomerType.Corporate,
                    ConnectionDate = DateTime.Now.AddMonths(-6),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                }
            };

            await _context.Customers.AddRangeAsync(customers);
        }

        private async Task SeedBillingRatesAsync()
        {
            var billingRates = new[]
            {
                new BillingRate
                {
                    CustomerType = CustomerType.Regular,
                    BaseRate = 5.50M,
                    ExcessRate = 7.00M,
                    Threshold = 100,
                    FixedCharges = 50,
                    EffectiveFrom = DateTime.Now.AddMonths(-3),
                    IsActive = true,
                    Notes = "Standard regular rate",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                },
                new BillingRate
                {
                    CustomerType = CustomerType.Premium,
                    BaseRate = 7.50M,
                    ExcessRate = 9.00M,
                    Threshold = 200,
                    FixedCharges = 100,
                    EffectiveFrom = DateTime.Now.AddMonths(-3),
                    IsActive = true,
                    Notes = "Standard premium rate",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                },
                new BillingRate
                {
                    CustomerType = CustomerType.Corporate,
                    BaseRate = 9.50M,
                    ExcessRate = 11.00M,
                    Threshold = 500,
                    FixedCharges = 200,
                    EffectiveFrom = DateTime.Now.AddMonths(-3),
                    IsActive = true,
                    Notes = "Standard corporate rate",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                }
            };

            await _context.BillingRates.AddRangeAsync(billingRates);
        }

        private async Task SeedMonthlyBillsAsync()
        {
            // Get customers
            var customers = await _context.Customers.ToListAsync();
            var random = new Random();

            foreach (var customer in customers)
            {
                // Create bills for the last 3 months
                for (int i = 1; i <= 3; i++)
                {
                    var billDate = DateTime.Now.AddMonths(-i);
                    var dueDate = billDate.AddDays(15);
                    var amount = random.Next(1000, 5000);
                    var tax = amount * 0.18M;
                    var total = amount + tax;

                    var bill = new MonthlyBill
                    {
                        CustomerId = customer.Id,
                        BillNumber = $"BILL-{customer.AccountNumber}-{billDate:yyyyMM}",
                        BillingMonth = billDate,
                        DueDate = dueDate,
                        Amount = amount,
                        TaxAmount = tax,
                        TotalAmount = total,
                        Status = i == 1 ? BillStatus.Pending : (i == 2 ? BillStatus.Paid : BillStatus.Overdue),
                        PresentReading = random.Next(1000, 2000),
                        PreviousReading = random.Next(500, 1000),
                        BlowerFanCharge = 100,
                        GeneratorCharge = 200,
                        ServiceCharge = 50,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system",
                        LastModifiedAt = DateTime.UtcNow,
                        LastModifiedBy = "system"
                    };

                    await _context.MonthlyBills.AddAsync(bill);
                }
            }
        }

        private async Task SeedPaymentRecordsAsync()
        {
            // Get paid bills
            var paidBills = await _context.MonthlyBills
                .Where(b => b.Status == BillStatus.Paid)
                .ToListAsync();

            foreach (var bill in paidBills)
            {
                var payment = new PaymentRecord
                {
                    MonthlyBillId = bill.Id,
                    CustomerId = bill.CustomerId,
                    PaymentDate = bill.BillingMonth.AddDays(10),
                    Amount = bill.TotalAmount,
                    AmountPaid = bill.TotalAmount,
                    PaymentMethod = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed,
                    ReferenceNumber = $"PAY-{bill.BillNumber}",
                    TransactionReference = $"TXN-{bill.BillNumber}",
                    Notes = "Regular payment",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                };

                await _context.PaymentRecords.AddAsync(payment);

                // Update bill status and payment reference
                bill.Status = BillStatus.Paid;
                bill.PaymentReference = payment.ReferenceNumber;
                bill.PaidDate = payment.PaymentDate;
            }
        }
    }
} 