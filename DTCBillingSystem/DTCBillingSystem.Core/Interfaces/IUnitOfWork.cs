using System;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IMeterReadingRepository MeterReadings { get; }
        IPrintJobRepository PrintJobs { get; }
        IMonthlyBillRepository MonthlyBills { get; }
        IPaymentRecordRepository PaymentRecords { get; }
        IAuditLogRepository AuditLogs { get; }
        IBackupInfoRepository BackupInfos { get; }
        IBackupScheduleRepository BackupSchedules { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
} 