using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class BillingPeriodRepository : BaseRepository<BillingPeriod>, IBillingPeriodRepository
    {
        public BillingPeriodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(x => x.Id == id);
        }

        public async Task<BillingPeriod?> GetCurrentBillingPeriodAsync()
        {
            return await _dbSet.OrderByDescending(b => b.StartDate)
                              .FirstOrDefaultAsync();
        }

        public async Task<BillingPeriod?> GetBillingPeriodByDateAsync(DateTime date)
        {
            return await _dbSet.Where(b => b.StartDate <= date && b.EndDate >= date)
                              .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BillingPeriod>> GetBillingPeriodsAsync(int pageNumber, int pageSize)
        {
            return await _dbSet.OrderByDescending(b => b.StartDate)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }

        public async Task<IEnumerable<BillingPeriod>> GetBillingPeriodsForYearAsync(int year)
        {
            return await _dbSet.Where(b => b.StartDate.Year == year)
                              .OrderByDescending(b => b.StartDate)
                              .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
} 