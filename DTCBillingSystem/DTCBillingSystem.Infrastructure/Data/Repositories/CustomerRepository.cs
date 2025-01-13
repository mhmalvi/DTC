using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer> GetCustomerWithBillsAsync(int customerId)
        {
            return await _dbSet
                .Include(c => c.Bills)
                .FirstOrDefaultAsync(c => c.Id == customerId);
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.ShopNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersByFloorAsync(string floor)
        {
            return await _dbSet
                .Where(c => c.Floor == floor && c.IsActive)
                .OrderBy(c => c.ShopNo)
                .ToListAsync();
        }

        public async Task<bool> IsShopNoUniqueAsync(string shopNo, int? excludeCustomerId = null)
        {
            var query = _dbSet.Where(c => c.ShopNo == shopNo);
            
            if (excludeCustomerId.HasValue)
                query = query.Where(c => c.Id != excludeCustomerId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveCustomersAsync();

            return await _dbSet
                .Where(c => c.IsActive &&
                           (c.Name.Contains(searchTerm) ||
                            c.ShopNo.Contains(searchTerm) ||
                            c.PhoneNumber.Contains(searchTerm) ||
                            c.Email.Contains(searchTerm)))
                .OrderBy(c => c.ShopNo)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Customer> Customers, int TotalCount)> GetCustomersWithBillsPagedAsync(
            int pageIndex,
            int pageSize,
            string searchTerm = null,
            string floor = null,
            bool? isActive = null)
        {
            IQueryable<Customer> query = _dbSet
                .Include(c => c.Bills.OrderByDescending(b => b.BillingMonth).Take(3));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    c.ShopNo.Contains(searchTerm) ||
                    c.PhoneNumber.Contains(searchTerm) ||
                    c.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(floor))
            {
                query = query.Where(c => c.Floor == floor);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var customers = await query
                .OrderBy(c => c.ShopNo)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalCount);
        }
    }
} 