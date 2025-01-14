using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPaymentRecordRepository : IRepository<PaymentRecord>
    {
        Task<IEnumerable<PaymentRecord>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByDateAsync(DateTime date);
        Task<IEnumerable<PaymentRecord>> GetCustomerPaymentsBeforeDateAsync(int customerId, DateTime date);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByBillIdAsync(int billId);
    }
} 