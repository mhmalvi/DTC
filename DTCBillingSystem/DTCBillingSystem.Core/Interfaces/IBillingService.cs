using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillingService
    {
        Task<MonthlyBill> GenerateBillAsync(int customerId, int userId);
        Task<PaymentRecord> ProcessPaymentAsync(int billId, decimal amount, PaymentMethod paymentMethod, string transactionId, int userId);
    }
} 