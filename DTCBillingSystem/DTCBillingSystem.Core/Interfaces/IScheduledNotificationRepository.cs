using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IScheduledNotificationRepository : IRepository<ScheduledNotification>
    {
        Task<IEnumerable<ScheduledNotification>> GetPendingNotificationsAsync();
        Task<IEnumerable<ScheduledNotification>> GetFailedNotificationsAsync();
        Task<IEnumerable<ScheduledNotification>> GetNotificationsDueByAsync(DateTime dueTime);
        Task<IEnumerable<ScheduledNotification>> GetNotificationsByStatusAsync(string status);
        Task<int> GetPendingNotificationsCountAsync();
    }
} 