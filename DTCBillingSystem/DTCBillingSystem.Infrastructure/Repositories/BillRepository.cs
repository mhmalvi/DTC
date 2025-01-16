using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class BillRepository : Repository<Bill>, IBillRepository
    {
        public BillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Bill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           b.BillingDate >= startDate && 
                           b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Bill>> GetOutstandingBillsAsync(DateTime asOfDate)
        {
            return await _dbSet
                .Where(b => b.DueDate <= asOfDate && b.Status == BillStatus.Unpaid)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<Bill?> GetLatestBillForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasBillsForCustomerAsync(int customerId)
        {
            return await _dbSet.AnyAsync(b => b.CustomerId == customerId);
        }

        public async Task<IEnumerable<Bill>> GetUnpaidBillsForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && b.Status == BillStatus.Unpaid)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalUnpaidAmountForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && b.Status == BillStatus.Unpaid)
                .SumAsync(b => b.Amount);
        }
    }
} 