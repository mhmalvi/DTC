using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface INotificationHistoryRepository : IRepository<NotificationHistory>
    {
        Task<IEnumerable<NotificationHistory>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<NotificationHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<NotificationHistory>> GetByStatusAsync(NotificationStatus status);
        Task<IEnumerable<NotificationHistory>> GetFailedNotificationsAsync();
        Task<IEnumerable<NotificationHistory>> GetPendingNotificationsAsync();
    }
} 