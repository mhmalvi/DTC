using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentRecord> RecordPaymentAsync(PaymentRecord payment);
        Task<PaymentRecord?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerAsync(int customerId);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByBillAsync(int billId);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaymentsForBillAsync(int billId);
        Task<bool> CancelPaymentAsync(int paymentId);
        Task<bool> RefundPaymentAsync(int paymentId, decimal amount, string reason);
    }
} 