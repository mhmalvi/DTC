using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface INotificationSettingsRepository : IRepository<NotificationSettings>
    {
        Task<NotificationSettings?> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<NotificationSettings>> GetByPreferenceAsync(NotificationPreference preference);
        Task<IEnumerable<NotificationSettings>> GetEnabledPushNotificationsAsync();
        Task<IEnumerable<NotificationSettings>> GetByDeviceTokenAsync(string deviceToken);
    }
} 