using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Customer> Customers { get; }
        IRepository<User> Users { get; }
        IRepository<MonthlyBill> MonthlyBills { get; }
        IRepository<PaymentRecord> PaymentRecords { get; }
        IRepository<MeterReading> MeterReadings { get; }
        IRepository<BillingRate> BillingRates { get; }
        IRepository<NotificationMessage> NotificationMessages { get; }
        IRepository<BackupSchedule> BackupSchedules { get; }
        IRepository<BackupInfo> BackupInfos { get; }
        IRepository<PrintJob> PrintJobs { get; }
        IRepository<AuditLog> AuditLogs { get; }

        Task<bool> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
} 