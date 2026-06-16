using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IMonthlyBillRepository : IRepository<MonthlyBill>
    {
        Task<IEnumerable<MonthlyBill>> GetBillsByCustomerAsync(Guid customerId);
        Task<IEnumerable<MonthlyBill>> GetBillsByPeriodAsync(string billingPeriod);
        Task<IEnumerable<MonthlyBill>> GetUnpaidBillsAsync();
        Task<IEnumerable<MonthlyBill>> GetOverdueBillsAsync();
        Task<decimal> GetTotalBilledAmountForPeriodAsync(DateTime startDate, DateTime endDate);
    }
} 