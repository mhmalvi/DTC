using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MonthlyBillRepository : Repository<MonthlyBill>, IMonthlyBillRepository
    {
        private new readonly ApplicationDbContext _context;

        public MonthlyBillRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<MonthlyBill?> GetByIdAsync(object id)
        {
            return await _context.MonthlyBills.FindAsync(id);
        }

        public async Task DeleteAsync(MonthlyBill entity)
        {
            _context.MonthlyBills.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByCustomerIdAsync(int customerId)
        {
            return await GetAllAsync(
                b => b.CustomerId == customerId,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(
                b => b.CustomerId == customerId && b.BillingDate >= startDate && b.BillingDate <= endDate,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(
                b => b.BillingDate >= startDate && b.BillingDate <= endDate,
                "Customer,PaymentRecords",
                false);
        }

        public async Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync()
        {
            return await GetAllAsync(
                b => b.Status == BillStatus.Pending,
                "Customer,PaymentRecords",
                false);
        }
    }
} 