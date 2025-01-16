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
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && 
                           p.PaymentDate >= startDate && 
                           p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBillIdAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.BillId == billId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetLatestPaymentForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPaymentsForCustomerAsync(int customerId)
        {
            return await _dbSet.AnyAsync(p => p.CustomerId == customerId);
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.BillId == billId && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }
    }
} 