using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationMessage> CreateNotificationAsync(NotificationMessage message, int userId);
    }
} 