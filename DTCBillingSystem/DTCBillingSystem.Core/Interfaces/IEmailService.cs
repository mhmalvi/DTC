using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath);
        bool ValidateEmailAddress(string email);
    }
} 