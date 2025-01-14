using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

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

        public virtual void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
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
    }

    public class MonthlyBillRepository : Repository<MonthlyBill>, IMonthlyBillRepository
    {
        public MonthlyBillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           b.BillingDate >= startDate && 
                           b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
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
                .Where(b => b.CustomerId == customerId && b.BillingDate <= date)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<MonthlyBill?> GetLatestBillAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BillingDate)
                .FirstOrDefaultAsync();
        }

        public new async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
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
                .Where(p => p.BillId == billId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }

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

        public async Task<IEnumerable<User>> GetAllAsync(UserRole? role = null)
        {
            var query = _dbSet.AsQueryable();
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);
            return await query.ToListAsync();
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

        public async Task<MeterReading?> GetLatestReadingAsync(int customerId)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
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
                .Where(n => n.Recipient == recipient)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
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

        public async Task<IEnumerable<BackupInfo>> GetByTypeAsync(string type)
        {
            return await _dbSet
                .Where(b => b.BackupType == type)
                .OrderByDescending(b => b.BackupDate)
                .ToListAsync();
        }

        public async Task<BackupInfo?> GetLatestBackupAsync()
        {
            return await _dbSet
                .OrderByDescending(b => b.BackupDate)
                .FirstOrDefaultAsync();
        }
    }

    public class BackupScheduleRepository : Repository<BackupSchedule>, IBackupScheduleRepository
    {
        public BackupScheduleRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync()
        {
            return await _dbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupSchedule>> GetByFrequencyAsync(BackupFrequency frequency)
        {
            return await _dbSet
                .Where(b => b.Frequency == frequency)
                .OrderBy(b => b.NextRunTime)
                .ToListAsync();
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