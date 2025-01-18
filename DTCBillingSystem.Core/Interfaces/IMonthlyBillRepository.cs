using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMonthlyBillRepository : IRepository<MonthlyBill>
    {
        Task<IEnumerable<MonthlyBill>> GetByCustomerIdAsync(int customerId);
    }
} 