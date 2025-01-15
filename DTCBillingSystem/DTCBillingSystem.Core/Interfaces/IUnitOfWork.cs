using System;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IMeterReadingRepository MeterReadings { get; }
        INotificationRepository Notifications { get; }
        IPrintJobRepository PrintJobs { get; }
        IMonthlyBillRepository MonthlyBills { get; }
        IPaymentRecordRepository PaymentRecords { get; }
        IAuditLogRepository AuditLogs { get; }
        IBackupInfoRepository BackupInfos { get; }
        INotificationSettingsRepository NotificationSettings { get; }
        IScheduledNotificationRepository ScheduledNotifications { get; }
        IBackupScheduleRepository BackupSchedules { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
} 