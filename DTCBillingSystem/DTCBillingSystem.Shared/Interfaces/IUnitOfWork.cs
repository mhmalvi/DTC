using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Customer> Customers { get; }
        IRepository<User> Users { get; }
        IRepository<MonthlyBill> MonthlyBills { get; }
        IRepository<PaymentRecord> PaymentRecords { get; }
        IRepository<PrintJob> PrintJobs { get; }
        IRepository<BackupSchedule> BackupSchedules { get; }
        IRepository<BackupInfo> Backups { get; }
        IMeterReadingRepository MeterReadings { get; }
        IBillingRateRepository BillingRates { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<NotificationMessage> Notifications { get; }

        Task<int> SaveChangesAsync();
    }
} 