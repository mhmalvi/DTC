using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using DTCBillingSystem.Shared.Interfaces;

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
        Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(string customerId);
        Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime dueDate);
        Task<IEnumerable<MonthlyBill>> GetForCustomerPeriodAsync(string customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetForPeriodAsync(DateTime startDate, DateTime endDate);
    }

    public interface IPaymentRepository : IRepository<PaymentRecord>
    {
        Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<PaymentRecord>> GetForDateAsync(DateTime date);
        Task<IEnumerable<PaymentRecord>> GetForPeriodAsync(DateTime startDate, DateTime endDate);
    }

    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
        Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetByActionAsync(AuditAction action, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string entityType = null,
            string entityId = null,
            AuditAction? action = null,
            string userId = null,
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
        Task<IEnumerable<MeterReading>> GetForBillingPeriodAsync(string customerId, DateTime billingMonth);
        Task<MeterReading> GetLatestReadingAsync(string customerId);
    }

    public interface INotificationHistoryRepository : IRepository<NotificationHistory>
    {
        Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<NotificationHistory>> GetByTypeAsync(NotificationType type);
    }

    public interface INotificationSettingsRepository : IRepository<NotificationSettings>
    {
        Task<NotificationSettings> GetByCustomerIdAsync(string customerId);
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
        Task<IEnumerable<BackupInfo>> GetByTypeAsync(string type);
        Task<BackupInfo> GetLatestBackupAsync();
    }

    public interface IBackupScheduleRepository : IRepository<BackupSchedule>
    {
        Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync();
        Task<IEnumerable<BackupSchedule>> GetByFrequencyAsync(BackupFrequency frequency);
    }
} 