using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Repositories
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

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedWithTotalAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>> predicate = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public virtual async Task<IEnumerable<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.CreatedDate >= startDate && e.CreatedDate <= endDate)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await GetByDateRangeAsync(startDate, endDate);
        }

        public virtual Task<IEnumerable<T>> GetForCustomerPeriodAsync(string customerId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }

        public virtual Task<IEnumerable<T>> GetOutstandingBillsAsync(string customerId)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }

        public virtual Task<IEnumerable<T>> GetBillsDueAsync(DateTime dueDate)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }

        public virtual Task<IEnumerable<T>> GetPaymentHistoryAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }

        public virtual Task<T> GetLatestReadingAsync(string customerId)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }

        public virtual Task<IEnumerable<T>> GetForBillingPeriodAsync(string customerId, DateTime billingMonth)
        {
            throw new NotImplementedException("This method should be implemented in specific repositories that need it.");
        }
    }
} 