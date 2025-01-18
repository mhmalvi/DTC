using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MonthlyBillRepository : BaseRepository<MonthlyBill>, IMonthlyBillRepository
    {
        public MonthlyBillRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MonthlyBill>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }
    }
} 