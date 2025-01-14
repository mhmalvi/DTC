using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Core.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
        {
            return await _dbSet.Where(u => u.Role == role).ToListAsync();
        }
    }

    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }

        public async Task<Customer> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
        }
    }

    public class BillRepository : BaseRepository<MonthlyBill>, IBillRepository
    {
        public BillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(int customerId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           (b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid))
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime dueDate)
        {
            return await _dbSet
                .Where(b => b.DueDate.Date == dueDate.Date && 
                           (b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid))
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetForCustomerPeriodAsync(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CustomerId == customerId && 
                           b.BillingMonth >= startDate && 
                           b.BillingMonth <= endDate)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillingMonth >= startDate && 
                           b.BillingMonth <= endDate)
                .OrderByDescending(b => b.BillingMonth)
                .ToListAsync();
        }
    }

    public class PaymentRepository : BaseRepository<PaymentRecord>, IPaymentRepository
    {
        public PaymentRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(p => p.Bill.CustomerId == customerId);

            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value);

            return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetForDateAsync(DateTime date)
        {
            return await _dbSet
                .Where(p => p.PaymentDate.Date == date.Date)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && 
                           p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }

    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId)
        {
            return await _dbSet
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(a => a.Action == action);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Timestamp >= startDate && 
                           a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string entityType = null,
            int? entityId = null,
            AuditAction? action = null,
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (action.HasValue)
                query = query.Where(a => a.Action == action.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
    }

    public class BillingRateRepository : BaseRepository<BillingRate>, IBillingRateRepository
    {
        public BillingRateRepository(DbContext context) : base(context) { }

        public async Task<BillingRate> GetForPeriodAsync(DateTime date)
        {
            return await _dbSet
                .Where(r => r.EffectiveFrom <= date && 
                           (!r.EffectiveTo.HasValue || r.EffectiveTo.Value >= date))
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BillingRate>> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(r => r.EffectiveFrom <= endDate && 
                           (!r.EffectiveTo.HasValue || r.EffectiveTo.Value >= startDate))
                .OrderByDescending(r => r.EffectiveFrom)
                .ToListAsync();
        }
    }

    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MeterReading>> GetForBillingPeriodAsync(int customerId, DateTime billingMonth)
        {
            var startDate = new DateTime(billingMonth.Year, billingMonth.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _dbSet
                .Where(r => r.CustomerId == customerId && 
                           r.ReadingDate >= startDate && 
                           r.ReadingDate <= endDate)
                .OrderByDescending(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<MeterReading> GetLatestReadingAsync(int customerId)
        {
            return await _dbSet
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefaultAsync();
        }
    }

    public class NotificationHistoryRepository : BaseRepository<NotificationHistory>, INotificationHistoryRepository
    {
        public NotificationHistoryRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(n => n.CustomerId == customerId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetByTypeAsync(NotificationType type)
        {
            return await _dbSet
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();
        }
    }

    public class NotificationSettingsRepository : BaseRepository<NotificationSettings>, INotificationSettingsRepository
    {
        public NotificationSettingsRepository(DbContext context) : base(context) { }

        public async Task<NotificationSettings> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet.FirstOrDefaultAsync(n => n.CustomerId == customerId);
        }
    }

    public class NotificationMessageRepository : BaseRepository<NotificationMessage>, INotificationMessageRepository
    {
        public NotificationMessageRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => !n.IsSent && 
                           (!n.ScheduledFor.HasValue || n.ScheduledFor.Value <= DateTime.UtcNow))
                .OrderBy(n => n.ScheduledFor)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetScheduledNotificationsAsync(DateTime before)
        {
            return await _dbSet
                .Where(n => !n.IsSent && 
                           n.ScheduledFor.HasValue && 
                           n.ScheduledFor.Value <= before)
                .OrderBy(n => n.ScheduledFor)
                .ToListAsync();
        }
    }

    public class PrintJobRepository : BaseRepository<PrintJob>, IPrintJobRepository
    {
        public PrintJobRepository(DbContext context) : base(context) { }

        public async Task<PrintJob> GetByJobIdAsync(string jobId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.JobId == jobId);
        }

        public async Task<IEnumerable<PrintJob>> GetPendingJobsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }
    }

    public class BackupInfoRepository : BaseRepository<BackupInfo>, IBackupInfoRepository
    {
        public BackupInfoRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<BackupInfo>> GetByTypeAsync(BackupType type)
        {
            return await _dbSet
                .Where(b => b.Type == type)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BackupInfo> GetLatestBackupAsync()
        {
            return await _dbSet
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }

    public class BackupScheduleRepository : BaseRepository<BackupSchedule>, IBackupScheduleRepository
    {
        public BackupScheduleRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync()
        {
            return await _dbSet
                .Where(s => s.Enabled)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupSchedule>> GetByFrequencyAsync(BackupFrequency frequency)
        {
            return await _dbSet
                .Where(s => s.Frequency == frequency)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }
    }
} 