using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IPaymentRecordRepository : IRepository<PaymentRecord>
    {
        Task<IEnumerable<PaymentRecord>> GetPaymentsByCustomerAsync(Guid customerId);
        Task<IEnumerable<PaymentRecord>> GetPaymentsByBillAsync(Guid billId);
        Task<decimal> GetTotalPaymentsForPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentRecord>> GetOverduePaymentsAsync();
    }
} 