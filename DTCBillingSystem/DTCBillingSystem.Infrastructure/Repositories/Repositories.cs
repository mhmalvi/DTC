using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
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

        public async Task<Customer?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
        }
    }

    public class BillRepository : BaseRepository<MonthlyBill>, IBillRepository
    {
        public BillRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(string accountNumber)
        {
            return await _dbSet
                .Where(b => b.Customer.AccountNumber == accountNumber && b.Status == BillStatus.Unpaid)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime dueDate)
        {
            return await _dbSet
                .Where(b => b.DueDate.Date == dueDate.Date && b.Status == BillStatus.Unpaid)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetForCustomerPeriodAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.Customer.AccountNumber == accountNumber && 
                           b.BillingPeriod >= startDate && b.BillingPeriod <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyBill>> GetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.BillingPeriod >= startDate && b.BillingPeriod <= endDate)
                .ToListAsync();
        }
    }

    public class PaymentRepository : BaseRepository<PaymentRecord>, IPaymentRepository
    {
        public PaymentRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(string accountNumber, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(p => p.Bill.Customer.AccountNumber == accountNumber);

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
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentRecord>> GetForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .ToListAsync();
        }
    }

    public class AuditLogRepository : BaseRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId)
        {
            return await _dbSet
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
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
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? entityType = null,
            string? entityId = null,
            AuditAction? action = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (!string.IsNullOrEmpty(entityId))
                query = query.Where(a => a.EntityId == entityId);

            if (action.HasValue)
                query = query.Where(a => a.Action == action.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
    }

    public class BillingRateRepository : BaseRepository<BillingRate>, IBillingRateRepository
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
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }

    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<MeterReading>> GetForBillingPeriodAsync(string accountNumber, DateTime billingDate)
        {
            return await _dbSet
                .Where(r => r.Customer.AccountNumber == accountNumber && r.ReadingDate.Month == billingDate.Month && r.ReadingDate.Year == billingDate.Year)
                .OrderByDescending(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<MeterReading?> GetLatestReadingAsync(string accountNumber)
        {
            return await _dbSet
                .Where(r => r.Customer.AccountNumber == accountNumber)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefaultAsync();
        }
    }

    public class NotificationHistoryRepository : BaseRepository<NotificationHistory>, INotificationHistoryRepository
    {
        public NotificationHistoryRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(string customerId)
        {
            return await _dbSet
                .Where(n => n.Customer.Id.ToString() == customerId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetByTypeAsync(NotificationType type)
        {
            return await _dbSet
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }

    public class NotificationSettingsRepository : BaseRepository<NotificationSettings>, INotificationSettingsRepository
    {
        public NotificationSettingsRepository(DbContext context) : base(context) { }

        public async Task<NotificationSettings?> GetByCustomerIdAsync(string customerId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(n => n.Customer.Id.ToString() == customerId);
        }
    }

    public class NotificationMessageRepository : BaseRepository<NotificationMessage>, INotificationMessageRepository
    {
        public NotificationMessageRepository(DbContext context) : base(context) { }

        public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetScheduledNotificationsAsync(DateTime scheduledDate)
        {
            return await _dbSet
                .Where(n => n.ScheduledDate.HasValue && n.ScheduledDate.Value.Date == scheduledDate.Date)
                .OrderBy(n => n.ScheduledDate)
                .ToListAsync();
        }
    }

    public class PrintJobRepository : BaseRepository<PrintJob>, IPrintJobRepository
    {
        public PrintJobRepository(DbContext context) : base(context) { }

        public async Task<PrintJob?> GetByJobIdAsync(string jobId)
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

        public async Task<IEnumerable<BackupInfo>> GetByTypeAsync(string backupType)
        {
            return await _dbSet
                .Where(b => b.BackupType == backupType)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BackupInfo?> GetLatestBackupAsync()
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
                .Where(s => s.IsActive)
                .OrderBy(s => s.NextRunTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupSchedule>> GetByFrequencyAsync(BackupFrequency frequency)
        {
            return await _dbSet
                .Where(s => s.Frequency == frequency)
                .OrderBy(s => s.NextRunTime)
                .ToListAsync();
        }
    }
} 