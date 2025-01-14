using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    }

    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<Customer> GetByAccountNumberAsync(string accountNumber);
    }

    public interface IBillRepository : IRepository<MonthlyBill>
    {
        Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(int customerId);
        Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime dueDate);
        Task<IEnumerable<MonthlyBill>> GetForCustomerPeriodAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetForPeriodAsync(DateTime startDate, DateTime endDate);
    }

    public interface IPaymentRepository : IRepository<PaymentRecord>
    {
        Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<PaymentRecord>> GetForDateAsync(DateTime date);
        Task<IEnumerable<PaymentRecord>> GetForPeriodAsync(DateTime startDate, DateTime endDate);
    }

    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId);
        Task<IEnumerable<AuditLog>> GetByUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string entityType = null,
            int? entityId = null,
            AuditAction? action = null,
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }

    public interface IBillingRateRepository : IRepository<BillingRate>
    {
        Task<BillingRate> GetForPeriodAsync(DateTime date);
        Task<IEnumerable<BillingRate>> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate);
    }

    public interface IMeterReadingRepository : IRepository<MeterReading>
    {
        Task<IEnumerable<MeterReading>> GetForBillingPeriodAsync(int customerId, DateTime billingMonth);
        Task<MeterReading> GetLatestReadingAsync(int customerId);
    }

    public interface INotificationHistoryRepository : IRepository<NotificationHistory>
    {
        Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<NotificationHistory>> GetByTypeAsync(NotificationType type);
    }

    public interface INotificationSettingsRepository : IRepository<NotificationSettings>
    {
        Task<NotificationSettings> GetByCustomerIdAsync(int customerId);
    }

    public interface INotificationMessageRepository : IRepository<NotificationMessage>
    {
        Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync();
        Task<IEnumerable<NotificationMessage>> GetScheduledNotificationsAsync(DateTime before);
    }

    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<PrintJob> GetByJobIdAsync(string jobId);
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
    }

    public interface IBackupInfoRepository : IRepository<BackupInfo>
    {
        Task<IEnumerable<BackupInfo>> GetByTypeAsync(BackupType type);
        Task<BackupInfo> GetLatestBackupAsync();
    }

    public interface IBackupScheduleRepository : IRepository<BackupSchedule>
    {
        Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync();
        Task<IEnumerable<BackupSchedule>> GetByFrequencyAsync(BackupFrequency frequency);
    }
} 