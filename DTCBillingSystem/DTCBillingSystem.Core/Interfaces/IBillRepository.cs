using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillRepository : IRepository<Bill>
    {
        Task<IEnumerable<Bill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Bill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Bill>> GetOutstandingBillsAsync(DateTime asOfDate);
        Task<Bill?> GetLatestBillForCustomerAsync(int customerId);
        Task<bool> HasBillsForCustomerAsync(int customerId);
        Task<IEnumerable<Bill>> GetUnpaidBillsForCustomerAsync(int customerId);
        Task<decimal> GetTotalUnpaidAmountForCustomerAsync(int customerId);
    }
} 