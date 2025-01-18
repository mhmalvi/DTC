using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Extensions
{
    public static class BillingRateRepositoryExtensions
    {
        public static async Task<BillingRate?> GetForPeriodAsync(this IRepository<BillingRate> repository, DateTime date)
        {
            var rates = await repository.GetAllAsync(
                filter: r => r.EffectiveFrom <= date && (r.EffectiveTo == null || r.EffectiveTo >= date) && r.IsActive,
                null,
                false);

            return rates.OrderByDescending(r => r.EffectiveFrom).FirstOrDefault();
        }

        public static async Task<IEnumerable<BillingRate>> GetHistoricalRatesAsync(
            this IRepository<BillingRate> repository,
            DateTime startDate,
            DateTime endDate)
        {
            var rates = await repository.GetAllAsync(
                filter: r => r.EffectiveFrom >= startDate && r.EffectiveFrom <= endDate,
                null,
                false);

            return rates.OrderByDescending(r => r.EffectiveFrom);
        }
    }
} 