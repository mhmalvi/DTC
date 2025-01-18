using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Extensions
{
    public static class MeterReadingRepositoryExtensions
    {
        public static async Task<MeterReading?> GetLatestReadingForCustomerAsync(
            this IRepository<MeterReading> repository,
            int customerId)
        {
            var readings = await repository.GetAllAsync(
                filter: r => r.CustomerId == customerId,
                null,
                false);

            return readings.OrderByDescending(r => r.ReadingDate).FirstOrDefault();
        }

        public static async Task<IEnumerable<MeterReading>> GetReadingsForPeriodAsync(
            this IRepository<MeterReading> repository,
            int customerId,
            DateTime startDate,
            DateTime endDate)
        {
            var readings = await repository.GetAllAsync(
                filter: r => r.CustomerId == customerId && r.ReadingDate >= startDate && r.ReadingDate <= endDate,
                null,
                false);

            return readings.OrderBy(r => r.ReadingDate);
        }
    }
} 