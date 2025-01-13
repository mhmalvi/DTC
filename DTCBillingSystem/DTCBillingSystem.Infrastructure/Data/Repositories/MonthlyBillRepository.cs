using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Repositories
{
    public class MonthlyBillRepository : BaseRepository<MonthlyBill>
    {
        public MonthlyBillRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MonthlyBill> GetBillWithDetailsAsync(int billId)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == billId);
        }

        public async Task<MonthlyBill> GetCustomerLatestBillAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId)
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
                           (b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid))
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

        public async Task<bool> HasBillForMonthAsync(int customerId, DateTime billingMonth)
        {
            return await _dbSet.AnyAsync(b => 
                b.CustomerId == customerId && 
                b.BillingMonth.Year == billingMonth.Year && 
                b.BillingMonth.Month == billingMonth.Month);
        }

        public async Task<(IEnumerable<MonthlyBill> Bills, int TotalCount)> GetBillsPagedAsync(
            int pageIndex,
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

            var totalCount = await query.CountAsync();
            var bills = await query
                .OrderByDescending(b => b.BillingMonth)
                .ThenBy(b => b.Customer.ShopNo)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bills, totalCount);
        }

        public async Task<decimal> GetTotalOutstandingAsync(int customerId)
        {
            var bills = await _dbSet
                .Include(b => b.Payments)
                .Where(b => b.CustomerId == customerId &&
                           (b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid))
                .ToListAsync();

            return bills.Sum(b => b.TotalAmount - b.Payments.Sum(p => p.AmountPaid));
        }
    }
} 