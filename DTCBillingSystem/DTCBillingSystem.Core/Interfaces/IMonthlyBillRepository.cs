using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMonthlyBillRepository : IRepository<MonthlyBill>
    {
        Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetCustomerBillsByDateRangeAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(DateTime asOfDate);
        Task<IEnumerable<MonthlyBill>> GetCustomerBillsBeforeDateAsync(int customerId, DateTime date);
        Task<MonthlyBill?> GetLatestBillAsync(int customerId);
        Task<int> CountAsync();
    }
} 