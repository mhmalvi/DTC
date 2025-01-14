using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>> predicate = null,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true)
        {
            IQueryable<T> query = _dbSet;

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
            {
                query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
    }

    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(DbContext context) : base(context)
        {
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<MeterReading> GetPreviousReadingForCustomerAsync(int customerId, DateTime currentReadingDate)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId && m.ReadingDate < currentReadingDate)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
        }
    }

    public class BillingRateRepository : BaseRepository<BillingRate>, IBillingRateRepository
    {
        public BillingRateRepository(DbContext context) : base(context)
        {
        }

        public async Task<BillingRate> GetByCustomerTypeAsync(CustomerType customerType)
        {
            return await _dbSet
                .Where(r => r.CustomerType == customerType && r.IsActive)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();
        }
    }

    public class PaymentRecordRepository : BaseRepository<PaymentRecord>, IPaymentRecordRepository
    {
        public PaymentRecordRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PaymentRecord>> GetForBillAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.BillId == billId)
                .ToListAsync();
        }
    }
} 