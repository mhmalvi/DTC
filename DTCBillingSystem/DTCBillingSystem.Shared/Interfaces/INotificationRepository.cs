using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface INotificationRepository : IRepository<NotificationMessage>
    {
        Task<IEnumerable<NotificationMessage>> GetUnsentNotificationsAsync();
        Task<IEnumerable<NotificationMessage>> GetByUserIdAsync(int userId);
        Task<IEnumerable<NotificationMessage>> GetByStatusAsync(string status);
    }
} 