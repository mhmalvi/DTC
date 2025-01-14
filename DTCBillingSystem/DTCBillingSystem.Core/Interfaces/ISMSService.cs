using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ISMSService
    {
        Task SendSMSAsync(string phoneNumber, string message);
        bool ValidatePhoneNumber(string phoneNumber);
        Task<bool> CheckDeliveryStatusAsync(string messageId);
    }
} 