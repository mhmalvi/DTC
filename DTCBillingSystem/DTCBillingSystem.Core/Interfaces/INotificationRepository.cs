using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface INotificationRepository : IRepository<NotificationMessage>
    {
        Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync();
        Task<IEnumerable<NotificationMessage>> GetFailedNotificationsAsync();
        Task<IEnumerable<NotificationMessage>> GetNotificationsByRecipientAsync(string recipient);
    }
} 