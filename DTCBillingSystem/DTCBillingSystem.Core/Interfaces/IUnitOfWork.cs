using System;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IMonthlyBillRepository MonthlyBills { get; }
        IPaymentRecordRepository PaymentRecords { get; }
        IBackupInfoRepository BackupInfos { get; }
        IPrintJobRepository PrintJobs { get; }
        IMeterReadingRepository MeterReadings { get; }
        IBackupScheduleRepository BackupSchedules { get; }
        IBillingRateRepository BillingRates { get; }
        IAuditLogRepository AuditLogs { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
} 