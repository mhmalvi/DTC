using System;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IMeterReadingRepository MeterReadings { get; }
        IBillingPeriodRepository BillingPeriods { get; }
        IMeterReadingScheduleRepository MeterReadingSchedules { get; }
        IUserRepository Users { get; }
        IInvoiceRepository Invoices { get; }
        IInvoiceItemRepository InvoiceItems { get; }
        IPaymentRepository Payments { get; }
        INotificationRepository Notifications { get; }
        IBackupRepository Backups { get; }
        IAuditLogRepository AuditLogs { get; }
        IMonthlyBillRepository MonthlyBills { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
} 