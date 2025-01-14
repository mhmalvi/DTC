using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Shared.Interfaces;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MonthlyBillRepository : BaseRepository<MonthlyBill>, IRepository<MonthlyBill>
    {
        public MonthlyBillRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MonthlyBill> GetBillWithDetailsAsync(string billId)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == billId);
        }

        public async Task<MonthlyBill> GetCustomerLatestBillAsync(string customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(string customerId)
        {
            return await _dbSet
                .Include(b => b.Payments)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetOverdueBillsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet
                .Include(b => b.Customer)
                .Where(b => b.DueDate < today && 
                           (b.Status == BillStatus.Overdue || b.Status == BillStatus.Generated))
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByMonthAsync(DateTime billingMonth)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Payments)
                .Where(b => b.BillingMonth.Year == billingMonth.Year && 
                           b.BillingMonth.Month == billingMonth.Month)
                .OrderBy(b => b.Customer.ShopNo)
                .ToListAsync();
        }

        public async Task<bool> HasBillForMonthAsync(string customerId, DateTime billingMonth)
        {
            return await _dbSet.AnyAsync(b => 
                b.CustomerId == customerId && 
                b.BillingMonth.Year == billingMonth.Year && 
                b.BillingMonth.Month == billingMonth.Month);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsPagedAsync(
            int pageNumber,
            int pageSize,
            DateTime? billingMonth = null,
            BillStatus? status = null,
            bool includeCustomer = false,
            bool includePayments = false)
        {
            IQueryable<MonthlyBill> query = _dbSet;

            if (includeCustomer)
                query = query.Include(b => b.Customer);

            if (includePayments)
                query = query.Include(b => b.Payments);

            if (billingMonth.HasValue)
            {
                query = query.Where(b => 
                    b.BillingMonth.Year == billingMonth.Value.Year && 
                    b.BillingMonth.Month == billingMonth.Value.Month);
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            return await query
                .OrderByDescending(b => b.BillingMonth)
                .ThenBy(b => b.Customer.ShopNo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalOutstandingAsync(string customerId)
        {
            var bills = await _dbSet
                .Include(b => b.Payments)
                .Where(b => b.CustomerId == customerId &&
                           (b.Status == BillStatus.Generated || b.Status == BillStatus.Overdue))
                .ToListAsync();

            return bills.Sum(b => b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
        }
    }
} 