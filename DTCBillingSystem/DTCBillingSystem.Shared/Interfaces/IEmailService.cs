using System.Threading.Tasks;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
} 