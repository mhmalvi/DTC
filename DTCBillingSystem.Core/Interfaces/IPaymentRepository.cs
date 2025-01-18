using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByCustomerAsync(int customerId);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId);
        Task<Payment?> GetLatestPaymentForCustomerAsync(int customerId);
        Task<bool> HasPaymentsForCustomerAsync(int customerId);
    }
} 