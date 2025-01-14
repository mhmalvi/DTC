using System.Threading.Tasks;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface ISMSService
    {
        Task SendSMSAsync(string phoneNumber, string message);
    }
} 