using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Infrastructure.Data;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class Repository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool trackChanges = false)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool trackChanges = false)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public virtual async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }
    }

    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
        {
            return await _dbSet
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            return await _dbSet
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private new readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByAccountNumberAsync(string accountNumber)
        {
            return await GetFirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
        }

        public async Task<Customer?> GetByMeterNumberAsync(string meterNumber)
        {
            return await GetFirstOrDefaultAsync(c => c.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type)
        {
            return await GetAllAsync(c => c.CustomerType == type);
        }

        public async Task<IEnumerable<Customer>> GetByNameAsync(string name)
        {
            return await _context.Customers
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> IsMeterNumberUniqueAsync(string meterNumber)
        {
            return !await AnyAsync(c => c.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(int skip, int take, string? searchTerm = null, CustomerType? type = null, bool? isActive = null, string? zone = null)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || 
                                       c.AccountNumber.Contains(searchTerm) || 
                                       c.MeterNumber.Contains(searchTerm));
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.CustomerType == type.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(zone))
            {
                query = query.Where(c => c.ZoneCode == zone);
            }

            return await query.OrderBy(c => c.Name)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null, CustomerType? type = null, bool? isActive = null)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || 
                                       c.AccountNumber.Contains(searchTerm) || 
                                       c.MeterNumber.Contains(searchTerm));
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.CustomerType == type.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null)
        {
            var query = _context.Customers.AsQueryable();
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            return !await query.AnyAsync(c => c.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zone)
        {
            return await GetAllAsync(c => c.ZoneCode == zone);
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                await RemoveAsync(customer);
            }
        }

        public async Task<bool> IsShopNoUniqueAsync(string shopNo, int? excludeCustomerId = null)
        {
            var query = _context.Customers.AsQueryable();
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            return !await query.AnyAsync(c => c.ShopNo == shopNo);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _context.Customers
                .Where(c => c.Name.Contains(searchTerm) || 
                           c.AccountNumber.Contains(searchTerm) || 
                           c.MeterNumber.Contains(searchTerm) ||
                           c.ShopNo.Contains(searchTerm))
                .OrderBy(c => c.Name)
                .Take(10)
                .ToListAsync();
        }
    }

    public class PaymentRecordRepository : Repository<PaymentRecord>, IPaymentRecordRepository
    {
        private new readonly ApplicationDbContext _context;

        public PaymentRecordRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByBillIdAsync(int billId)
        {
            return await GetAllAsync(p => p.MonthlyBillId == billId, "MonthlyBill");
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            return await GetAllAsync(p => p.MonthlyBill.CustomerId == customerId, "MonthlyBill");
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate, "MonthlyBill");
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && 
                     p.PaymentDate >= startDate && 
                     p.PaymentDate <= endDate,
                "MonthlyBill"
            );
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateAsync(DateTime date)
        {
            return await GetAllAsync(
                p => p.PaymentDate.Date == date.Date,
                "MonthlyBill"
            );
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsBeforeDateAsync(int customerId, DateTime date)
        {
            return await GetAllAsync(
                p => p.MonthlyBill.CustomerId == customerId && 
                     p.PaymentDate < date,
                "MonthlyBill"
            );
        }

        public async Task<bool> HasPaymentsForCustomerAsync(int customerId)
        {
            return await AnyAsync(p => p.MonthlyBill.CustomerId == customerId);
        }

        public async Task<PaymentRecord?> GetLatestPaymentForCustomerAsync(int customerId)
        {
            return await _context.PaymentRecords
                .Include(p => p.MonthlyBill)
                .Where(p => p.MonthlyBill.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _context.PaymentRecords
                .Where(p => p.MonthlyBillId == billId)
                .SumAsync(p => p.Amount);
        }
    }

    public class MeterReadingRepository : Repository<MeterReading>, IMeterReadingRepository
    {
        private new readonly ApplicationDbContext _context;

        public MeterReadingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsForPeriodAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(
                r => r.CustomerId == customerId && r.ReadingDate >= startDate && r.ReadingDate <= endDate,
                "Customer"
            );
        }

        public async Task<MeterReading?> GetLatestReadingAsync(int customerId)
        {
            return await GetFirstOrDefaultAsync(
                r => r.CustomerId == customerId,
                "Customer",
                true
            );
        }

        public async Task<MeterReading?> GetLatestReadingForCustomerAsync(int customerId)
        {
            return await _context.MeterReadings
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            var query = _context.MeterReadings
                .Where(r => r.CustomerId == customerId);
            return Task.FromResult(query);
        }
    }
} 