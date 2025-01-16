using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payment>> GetCustomerPaymentsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payment>> GetPaymentsByBillIdAsync(int billId);
        Task<Payment?> GetLatestPaymentForCustomerAsync(int customerId);
        Task<bool> HasPaymentsForCustomerAsync(int customerId);
        Task<decimal> GetTotalPaymentsForBillAsync(int billId);
    }
} 