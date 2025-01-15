using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Entities;

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
        public CustomerRepository(DbContext context) : base(context) { }

        public async Task<Customer?> GetByMeterNumberAsync(string meterNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> IsMeterNumberUniqueAsync(string meterNumber)
        {
            return !await _dbSet.AnyAsync(c => c.MeterNumber == meterNumber);
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
                query = query.Where(c => c.Name.Contains(searchText) || 
                                       c.MeterNumber.Contains(searchText) ||
                                       c.PhoneNumber.Contains(searchText));
            }

            if (customerType.HasValue)
            {
                query = query.Where(c => c.CustomerType == customerType.Value);
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
                    case "meternumber":
                        query = query.OrderBy(c => c.MeterNumber);
                        break;
                    case "registrationdate":
                        query = query.OrderByDescending(c => c.RegistrationDate);
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
                query = query.Where(c => c.Name.Contains(searchText) || 
                                       c.MeterNumber.Contains(searchText) ||
                                       c.PhoneNumber.Contains(searchText));
            }

            if (customerType.HasValue)
            {
                query = query.Where(c => c.CustomerType == customerType.Value);
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
                .Where(c => c.ZoneCode == zoneCode && c.IsActive)
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
    }

    public class MonthlyBillRepository : Repository<MonthlyBill>, IMonthlyBillRepository
    {
        public MonthlyBillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillDate >= startDate && b.BillDate <= endDate)
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           b.BillDate >= startDate && 
                           b.BillDate <= endDate)
                .OrderByDescending(b => b.BillDate)
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
                .Where(b => b.CustomerId == customerId && b.BillDate <= date)
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetLatestBillAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillDate)
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
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetLatestBillForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillDate)
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
    }

    public class PaymentRecordRepository : Repository<PaymentRecord>, IPaymentRecordRepository
    {
        public PaymentRecordRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && 
                           p.PaymentDate >= startDate && 
                           p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(p => p.PaymentDate.Date == date.Date)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsBeforeDateAsync(int customerId, DateTime date)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && p.PaymentDate < date)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByBillIdAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.MonthlyBillId == billId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
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
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalPaymentsForBillAsync(int billId)
        {
            return await _dbSet
                .Where(p => p.MonthlyBillId == billId && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();
            return await query.ToListAsync();
        }

        public override async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public override async Task AddAsync(User entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<User> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(User entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public override async Task RemoveAsync(User entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<User> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _dbSet.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _dbSet.AnyAsync(u => u.Email == email);
        }
    }

    public class BillingRateRepository : Repository<BillingRate>, IRepository<BillingRate>
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

    public class ScheduledNotificationRepository : Repository<ScheduledNotification>, IScheduledNotificationRepository
    {
        public ScheduledNotificationRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<ScheduledNotification>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.ScheduledTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduledNotification>> GetFailedNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Failed)
                .OrderByDescending(n => n.LastRetryTime ?? n.ScheduledTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduledNotification>> GetNotificationsDueByAsync(DateTime dueTime)
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Pending && n.ScheduledTime <= dueTime)
                .OrderBy(n => n.ScheduledTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduledNotification>> GetNotificationsByStatusAsync(string status)
        {
            return await _dbSet
                .Where(n => n.Status.ToString() == status)
                .OrderByDescending(n => n.ScheduledTime)
                .ToListAsync();
        }

        public async Task<int> GetPendingNotificationsCountAsync()
        {
            return await _dbSet.CountAsync(n => n.Status == NotificationStatus.Pending);
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

        public async Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            return _dbSet.Where(m => m.CustomerId == customerId);
        }

        public new async Task UpdateAsync(MeterReading reading)
        {
            _context.Entry(reading).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public new async Task RemoveAsync(MeterReading reading)
        {
            _dbSet.Remove(reading);
            await Task.CompletedTask;
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

    public class NotificationHistoryRepository : Repository<NotificationHistory>, INotificationHistoryRepository
    {
        public NotificationHistoryRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(n => n.CreatedAt >= startDate && n.CreatedAt <= endDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetByStatusAsync(NotificationStatus status)
        {
            return await _dbSet
                .Where(n => n.Status == status)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetFailedNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Failed)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }

    public class NotificationSettingsRepository : Repository<NotificationSettings>, INotificationSettingsRepository
    {
        public NotificationSettingsRepository(DbContext context) : base(context) { }

        public async Task<NotificationSettings?> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(n => n.CustomerId == customerId);
        }

        public async Task<IEnumerable<NotificationSettings>> GetByPreferenceAsync(NotificationPreference preference)
        {
            return await _dbSet
                .Where(n => n.Preference == preference)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationSettings>> GetEnabledPushNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.IsPushEnabled)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationSettings>> GetByDeviceTokenAsync(string deviceToken)
        {
            return await _dbSet
                .Where(n => n.DeviceToken == deviceToken)
                .ToListAsync();
        }
    }

    public class NotificationMessageRepository : Repository<NotificationMessage>, INotificationRepository
    {
        public NotificationMessageRepository(DbContext context) : base(context) { }

        // INotificationRepository specific methods
        public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetFailedNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Failed)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetNotificationsByRecipientAsync(string recipient)
        {
            return await _dbSet
                .Where(n => n.RecipientEmail == recipient || n.RecipientPhoneNumber == recipient)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Base IRepository<NotificationMessage> methods
        public override async Task<NotificationMessage?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task<IEnumerable<NotificationMessage>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<IEnumerable<NotificationMessage>> FindAsync(Expression<Func<NotificationMessage, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public override async Task AddAsync(NotificationMessage entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<NotificationMessage> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(NotificationMessage entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public override async Task RemoveAsync(NotificationMessage entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<NotificationMessage> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<NotificationMessage, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<NotificationMessage, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
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

    public class BackupInfoRepository : Repository<BackupInfo>, IBackupInfoRepository
    {
        public BackupInfoRepository(DbContext context) : base(context) { }

        public override async Task<BackupInfo?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task<IEnumerable<BackupInfo>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<IEnumerable<BackupInfo>> FindAsync(Expression<Func<BackupInfo, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public override async Task AddAsync(BackupInfo entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<BackupInfo> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(BackupInfo entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public override async Task RemoveAsync(BackupInfo entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<BackupInfo> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<BackupInfo, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<BackupInfo, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<BackupInfo>> GetByStatusAsync(BackupStatus status)
        {
            return await _dbSet
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupInfo>> GetLatestBackupsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(b => b.StartTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<BackupInfo?> GetLatestSuccessfulBackupAsync()
        {
            return await _dbSet
                .Where(b => b.Status == BackupStatus.Completed)
                .OrderByDescending(b => b.StartTime)
                .FirstOrDefaultAsync();
        }
    }

    public class BackupScheduleRepository : Repository<BackupSchedule>, IBackupScheduleRepository
    {
        public BackupScheduleRepository(DbContext context) : base(context) { }

        public override async Task<BackupSchedule?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task<IEnumerable<BackupSchedule>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task<IEnumerable<BackupSchedule>> FindAsync(Expression<Func<BackupSchedule, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public override async Task AddAsync(BackupSchedule entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public override async Task AddRangeAsync(IEnumerable<BackupSchedule> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public override async Task UpdateAsync(BackupSchedule entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public override async Task RemoveAsync(BackupSchedule entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public override async Task RemoveRangeAsync(IEnumerable<BackupSchedule> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public override async Task<bool> AnyAsync(Expression<Func<BackupSchedule, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public override async Task<int> CountAsync(Expression<Func<BackupSchedule, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync()
        {
            return await _dbSet
                .Where(b => b.IsEnabled)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
        }

        public async Task<int> GetActiveSchedulesCountAsync()
        {
            return await _dbSet.CountAsync(b => b.IsEnabled);
        }

        public async Task<IEnumerable<BackupSchedule>> GetSchedulesDueByAsync(DateTime dueTime)
        {
            return await _dbSet
                .Where(b => b.IsEnabled && b.NextRunTime <= dueTime)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
        }

        public async Task<BackupSchedule?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(b => b.Name == name);
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

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId)
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