using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Service interface for SMS operations
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// Send a single SMS message
        /// </summary>
        Task SendSMSAsync(NotificationMessage message);

        /// <summary>
        /// Send multiple SMS messages in bulk
        /// </summary>
        Task SendBulkSMSAsync(IEnumerable<NotificationMessage> messages);
    }
} 