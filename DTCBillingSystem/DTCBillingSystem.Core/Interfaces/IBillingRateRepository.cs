using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBillingRateRepository : IRepository<BillingRate>
    {
        Task<BillingRate?> GetForPeriodAsync(DateTime date);
        Task<IEnumerable<BillingRate>> GetHistoricalRatesAsync(DateTime startDate, DateTime endDate);
    }
} 