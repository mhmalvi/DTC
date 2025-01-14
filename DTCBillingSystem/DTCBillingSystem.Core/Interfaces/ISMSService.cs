using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ISMSService
    {
        Task SendSMSAsync(NotificationMessage message);
        Task SendBulkSMSAsync(IEnumerable<NotificationMessage> messages);
    }
} 