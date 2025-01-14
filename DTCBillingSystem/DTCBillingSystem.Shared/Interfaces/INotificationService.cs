using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationMessage> CreateNotificationAsync(NotificationMessage notification, int userId);
        Task UpdateNotificationStatusAsync(int notificationId, string status, int userId);
        Task DeleteNotificationAsync(int notificationId, int userId);
    }
} 