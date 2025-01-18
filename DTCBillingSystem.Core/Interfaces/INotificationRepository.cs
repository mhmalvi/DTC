using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync();
        Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type);
        Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId);
    }
} 