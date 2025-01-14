using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userId);
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
} 