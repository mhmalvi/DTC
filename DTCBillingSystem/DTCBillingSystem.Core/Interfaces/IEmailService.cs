using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(NotificationMessage message);
        Task SendBulkEmailAsync(IEnumerable<NotificationMessage> messages);
    }
} 