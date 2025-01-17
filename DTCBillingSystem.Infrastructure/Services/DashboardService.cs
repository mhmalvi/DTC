using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<MonthlyBill> RecentBills, IEnumerable<PaymentRecord> RecentPayments, 
            int TotalCustomers, decimal TotalRevenue, int PendingBills)> GetDashboardDataAsync()
        {
            try
            {
                var recentBills = await _context.MonthlyBills
                    .Include(b => b.Customer)
                    .OrderByDescending(b => b.BillingMonth)
                    .Take(5)
                    .ToListAsync();

                var recentPayments = await _context.PaymentRecords
                    .Include(p => p.Customer)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync();

                var totalCustomers = await _context.Customers.CountAsync();
                var totalRevenue = await _context.PaymentRecords.SumAsync(p => p.AmountPaid);
                var pendingBills = await _context.MonthlyBills.CountAsync(b => b.Status == Core.Models.Enums.BillStatus.Pending);

                return (recentBills, recentPayments, totalCustomers, totalRevenue, pendingBills);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load dashboard data", ex);
            }
        }
    }
} 