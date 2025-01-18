using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillingPeriodRepository : IRepository<BillingPeriod>
    {
        Task<BillingPeriod?> GetCurrentBillingPeriodAsync();
        Task<BillingPeriod?> GetBillingPeriodByDateAsync(DateTime date);
        Task<IEnumerable<BillingPeriod>> GetBillingPeriodsAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();
    }
} 