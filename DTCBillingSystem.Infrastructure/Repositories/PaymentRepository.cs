using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId)
        {
            return await _dbSet
                .Where(p => p.InvoiceId == invoiceId && !p.IsVoid)
                .SumAsync(p => p.Amount);
        }

        public async Task<Payment?> GetLatestPaymentForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && !p.IsVoid)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasPaymentsForCustomerAsync(int customerId)
        {
            return await _dbSet
                .AnyAsync(p => p.CustomerId == customerId && !p.IsVoid);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId)
        {
            return await _dbSet
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetByReferenceNumberAsync(string referenceNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ReferenceNumber == referenceNumber);
        }
    }
} 