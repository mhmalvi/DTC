using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMonthlyBillRepository : IBaseRepository<MonthlyBill>
    {
        Task<IEnumerable<MonthlyBill>> GetBillsByCustomerIdAsync(int customerId);
        Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetBillsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync();
    }
} 