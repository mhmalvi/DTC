using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> RecordPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(int customerId);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId);
        Task<bool> VoidPaymentAsync(int paymentId, int userId, string reason);
    }
} 