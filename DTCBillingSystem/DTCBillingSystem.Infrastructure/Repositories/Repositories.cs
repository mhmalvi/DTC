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
    public class Repository<T> : IRepository<T> where T : class
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

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
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

    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly DbSet<MeterReading> _meterReadings;

        public CustomerRepository(DbContext context) : base(context)
        {
            _meterReadings = context.Set<MeterReading>();
        }

        public async Task<Customer?> GetByMeterNumberAsync(string meterNumber)
        {
            var meterReading = await _meterReadings
                .Include(m => m.Customer)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync(m => m.MeterNumber == meterNumber);

            return meterReading?.Customer;
        }

        public async Task<IEnumerable<Customer>> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> IsMeterNumberUniqueAsync(string meterNumber)
        {
            return !await _meterReadings.AnyAsync(m => m.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string? sortBy = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var meterNumbers = await _meterReadings
                    .Where(m => m.MeterNumber.Contains(searchText))
                    .Select(m => m.CustomerId)
                    .ToListAsync();

                query = query.Where(c => c.Name.Contains(searchText) || 
                                       c.AccountNumber.Contains(searchText) ||
                                       c.PhoneNumber.Contains(searchText) ||
                                       meterNumbers.Contains(c.Id));
            }

            if (customerType.HasValue)
            {
                query = query.Where(c => c.Status == (CustomerStatus)customerType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = query.OrderBy(c => c.Name);
                        break;
                    case "accountnumber":
                        query = query.OrderBy(c => c.AccountNumber);
                        break;
                    case "createdat":
                        query = query.OrderByDescending(c => c.CreatedAt);
                        break;
                    default:
                        query = query.OrderBy(c => c.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.Id);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var meterNumbers = await _meterReadings
                    .Where(m => m.MeterNumber.Contains(searchText))
                    .Select(m => m.CustomerId)
                    .ToListAsync();

                query = query.Where(c => c.Name.Contains(searchText) || 
                                       c.AccountNumber.Contains(searchText) ||
                                       c.PhoneNumber.Contains(searchText) ||
                                       meterNumbers.Contains(c.Id));
            }

            if (customerType.HasValue)
            {
                query = query.Where(c => c.Status == (CustomerStatus)customerType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            return !await query.AnyAsync(c => c.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode)
        {
            return await _dbSet
                .Where(c => c.IsActive && c.ZoneCode == zoneCode)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _dbSet.FindAsync(id);
            if (customer != null)
            {
                customer.IsActive = false;
                customer.LastModifiedAt = DateTime.UtcNow;
                _context.Entry(customer).State = EntityState.Modified;
            }
        }

        public async Task<bool> IsShopNoUniqueAsync(string shopNo, int? excludeCustomerId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            }
            return !await query.AnyAsync(c => c.ShopNo == shopNo);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _dbSet.OrderBy(c => c.Name).ToListAsync();

            var meterNumbers = await _meterReadings
                .Where(m => m.MeterNumber != null && m.MeterNumber.Contains(searchTerm))
                .Select(m => m.CustomerId)
                .ToListAsync();

            return await _dbSet
                .Where(c => (c.Name != null && c.Name.Contains(searchTerm)) || 
                           (c.AccountNumber != null && c.AccountNumber.Contains(searchTerm)) ||
                           (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm)) ||
                           (c.ShopNo != null && c.ShopNo.Contains(searchTerm)) ||
                           meterNumbers.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }

    public class MonthlyBillRepository : Repository<MonthlyBill>, IMonthlyBillRepository
    {
        public MonthlyBillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillingMonth >= startDate && b.BillingMonth <= endDate)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           b.BillingMonth >= startDate && 
                           b.BillingMonth <= endDate)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(DateTime asOfDate)
        {
            return await _dbSet
                .Where(b => b.Status == BillStatus.Unpaid && b.DueDate <= asOfDate)
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsBeforeDateAsync(int customerId, DateTime date)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && b.BillingMonth <= date)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetLatestBillAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetLatestBillForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasBillsForCustomerAsync(int customerId)
        {
            return await _dbSet.AnyAsync(b => b.CustomerId == customerId);
        }

        public async Task<IEnumerable<MonthlyBill>> GetUnpaidBillsForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && b.Status == BillStatus.Unpaid)
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalUnpaidAmountForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && b.Status == BillStatus.Unpaid)
                .SumAsync(b => b.TotalAmount);
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetCustomerLatestBillAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsForMonthAsync(int customerId, DateTime billingMonth)
        {
            return await _dbSet.AnyAsync(b => 
                b.CustomerId == customerId && 
                b.BillingMonth.Year == billingMonth.Year && 
                b.BillingMonth.Month == billingMonth.Month);
        }

        public async Task<IEnumerable<MonthlyBill>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var bill = await _dbSet.FindAsync(id);
            if (bill != null)
            {
                _dbSet.Remove(bill);
            }
        }
    }

    public class PaymentRecordRepository : Repository<PaymentRecord>, IPaymentRecordRepository
    {
        public PaymentRecordRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && 
                           p.CreatedAt >= startDate && 
                           p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(p => p.CreatedAt.Date == date.Date)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsBeforeDateAsync(int customerId, DateTime date)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && p.CreatedAt < date)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByBillIdAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.MonthlyBillId == billId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPaymentsForCustomerAsync(int customerId)
        {
            return await _dbSet.AnyAsync(p => p.CustomerId == customerId);
        }

        public async Task<PaymentRecord?> GetLatestPaymentForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.MonthlyBillId == billId && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }
    }

    public class UserRepository : Repository<Core.Models.Entities.User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }

        public override async Task<Core.Models.Entities.User?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null ? MapToModel(entity) : null;
        }

        public override async Task<IEnumerable<Core.Models.Entities.User>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<Core.Models.Entities.User?> FindByUsernameAsync(string username)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
            return entity != null ? MapToModel(entity) : null;
        }

        public async Task<Core.Models.Entities.User?> FindByEmailAsync(string email)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
            return entity != null ? MapToModel(entity) : null;
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _dbSet.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Core.Models.Entities.User>> GetActiveUsersAsync()
        {
            var entities = await _dbSet.Where(u => u.Status == UserStatus.Active).ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<IEnumerable<Core.Models.Entities.User>> GetUsersByRoleAsync(string role)
        {
            if (!Enum.TryParse<UserRole>(role, out var userRole))
                return Enumerable.Empty<Core.Models.Entities.User>();

            var entities = await _dbSet.Where(u => u.Role == userRole).ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<bool> UpdateLastLoginAsync(int userId, string? ipAddress = null)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;

            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLoginAttemptsAsync(int userId, int attempts)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;

            user.LoginAttempts = attempts;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LockUserAsync(int userId, DateTime lockUntil)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;

            user.Status = UserStatus.Locked;
            user.LockedUntil = lockUntil;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null) return false;

            user.Status = UserStatus.Active;
            user.LockedUntil = null;
            user.LoginAttempts = 0;
            await _context.SaveChangesAsync();
            return true;
        }

        private static Core.Models.Entities.User MapToModel(Core.Models.Entities.User entity)
        {
            return new Core.Models.Entities.User
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                Password = entity.Password,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                PhoneNumber = entity.PhoneNumber,
                Role = entity.Role,
                Status = entity.Status,
                LastLoginAt = entity.LastLoginAt,
                LastLoginIp = entity.LastLoginIp,
                LoginAttempts = entity.LoginAttempts,
                LockedUntil = entity.LockedUntil,
                RefreshToken = entity.RefreshToken,
                RefreshTokenExpiryTime = entity.RefreshTokenExpiryTime,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }

    public class BillingRateRepository : Repository<BillingRate>, IBillingRateRepository
    {
        public BillingRateRepository(DbContext context) : base(context) { }

        public async Task<BillingRate?> GetForPeriodAsync(DateTime date)
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BillingRate>> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(r => r.CreatedAt >= startDate && 
                           r.CreatedAt <= endDate)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

    public class MeterReadingRepository : Repository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(DbContext context) : base(context) { }

        public async Task<MeterReading?> GetLatestReadingForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            return Task.FromResult(_dbSet.Where(m => m.CustomerId == customerId));
        }

        public override async Task UpdateAsync(MeterReading reading)
        {
            _context.Entry(reading).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public override async Task RemoveAsync(MeterReading reading)
        {
            _dbSet.Remove(reading);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsForPeriodAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId && 
                           m.ReadingDate >= startDate && 
                           m.ReadingDate <= endDate)
                .OrderByDescending(m => m.ReadingDate)
                .ToListAsync();
        }

        public async Task<bool> HasReadingForDateAsync(int customerId, DateTime date)
        {
            return await _dbSet.AnyAsync(m => 
                m.CustomerId == customerId && 
                m.ReadingDate.Date == date.Date);
        }
    }

    public class PrintJobRepository : Repository<PrintJob>, IPrintJobRepository
    {
        public PrintJobRepository(DbContext context) : base(context) { }

        public async Task<PrintJob?> GetByJobIdAsync(string jobId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.JobId == jobId);
        }

        public async Task<IEnumerable<PrintJob>> GetPendingJobsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PrintJobStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetFailedJobsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PrintJobStatus.Failed)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(PrintJobStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }
    }

    public class BackupInfoRepository : Repository<Core.Models.Entities.BackupInfo>, IBackupInfoRepository
    {
        public BackupInfoRepository(DbContext context) : base(context) { }

        public override async Task<Core.Models.Entities.BackupInfo?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null ? MapToModel(entity) : null;
        }

        public override async Task<IEnumerable<Core.Models.Entities.BackupInfo>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return entities.Select(MapToModel);
        }

        public override async Task<IEnumerable<Core.Models.Entities.BackupInfo>> FindAsync(Expression<Func<Core.Models.Entities.BackupInfo, bool>> predicate)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();
            return entities.Select(MapToModel);
        }

        public override async Task AddAsync(Core.Models.Entities.BackupInfo entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<Core.Models.Entities.BackupInfo> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(Core.Models.Entities.BackupInfo entity)
        {
            var existingEntity = await _dbSet.FindAsync(entity.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                existingEntity.UpdatedAt = DateTime.UtcNow;
            }
        }

        public override async Task RemoveAsync(Core.Models.Entities.BackupInfo entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<Core.Models.Entities.BackupInfo> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<Core.Models.Entities.BackupInfo, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<Core.Models.Entities.BackupInfo, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<Core.Models.Entities.BackupInfo>> GetByStatusAsync(BackupStatus status)
        {
            var entities = await _dbSet
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return entities.Select(MapToModel);
        }


        public async Task<IEnumerable<Core.Models.Entities.BackupInfo>> GetLatestBackupsAsync(int count)
        {
            var entities = await _dbSet
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<Core.Models.Entities.BackupInfo?> GetLatestSuccessfulBackupAsync()
        {
            var entity = await _dbSet
                .Where(b => b.Status == BackupStatus.Completed)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
            return entity != null ? MapToModel(entity) : null;
        }

        private static Core.Models.Entities.BackupInfo MapToModel(Core.Models.Entities.BackupInfo entity)
        {
            return new Core.Models.Entities.BackupInfo
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                FilePath = entity.FilePath,
                FileSize = entity.FileSize,
                Status = entity.Status,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsCompressed = entity.IsCompressed,
                IncludesTransactionLogs = entity.IncludesTransactionLogs,
                DatabaseVersion = entity.DatabaseVersion,
                IsVerified = entity.IsVerified,
                ErrorMessage = entity.ErrorMessage,
                CreatedAt = entity.CreatedAt,
                LastModifiedAt = entity.LastModifiedAt,
                LastModifiedBy = entity.LastModifiedBy
            };
        }
    }

    public class BackupScheduleRepository : Repository<Core.Models.Entities.BackupSchedule>, IBackupScheduleRepository
    {
        public BackupScheduleRepository(DbContext context) : base(context) { }

        public override async Task<Core.Models.Entities.BackupSchedule?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null ? MapToModel(entity) : null;
        }

        public override async Task<IEnumerable<Core.Models.Entities.BackupSchedule>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return entities.Select(MapToModel);
        }

        public override async Task<IEnumerable<Core.Models.Entities.BackupSchedule>> FindAsync(Expression<Func<Core.Models.Entities.BackupSchedule, bool>> predicate)
        {
            var entities = await _dbSet.Where(predicate).ToListAsync();
            return entities.Select(MapToModel);
        }

        public override async Task AddAsync(Core.Models.Entities.BackupSchedule entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<Core.Models.Entities.BackupSchedule> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(Core.Models.Entities.BackupSchedule entity)
        {
            var existingEntity = await _dbSet.FindAsync(entity.Id);
            if (existingEntity != null)
            {
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                existingEntity.UpdatedAt = DateTime.UtcNow;
            }
        }

        public override async Task RemoveAsync(Core.Models.Entities.BackupSchedule entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<Core.Models.Entities.BackupSchedule> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<Core.Models.Entities.BackupSchedule, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<Core.Models.Entities.BackupSchedule, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<Core.Models.Entities.BackupSchedule>> GetActiveSchedulesAsync()
        {
            var entities = await _dbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<int> GetActiveSchedulesCountAsync()
        {
            return await _dbSet.CountAsync(b => b.IsActive);
        }

        public async Task<IEnumerable<Core.Models.Entities.BackupSchedule>> GetSchedulesDueByAsync(DateTime dueTime)
        {
            var entities = await _dbSet
                .Where(b => b.IsActive && b.NextRunTime <= dueTime)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
            return entities.Select(MapToModel);
        }

        public async Task<Core.Models.Entities.BackupSchedule?> GetByNameAsync(string name)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
            return entity != null ? MapToModel(entity) : null;
        }

        private static Core.Models.Entities.BackupSchedule MapToModel(Core.Models.Entities.BackupSchedule entity)
        {
            return new Core.Models.Entities.BackupSchedule
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CronExpression = entity.CronExpression,
                IsActive = entity.IsActive,
                LastRunTime = entity.LastRunTime,
                NextRunTime = entity.NextRunTime,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
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
} 