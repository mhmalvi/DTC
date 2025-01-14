using System;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;

namespace DTCBillingSystem.Core.Extensions
{
    public static class MeterReadingRepositoryExtensions
    {
        public static async Task<MeterReading?> GetLatestReadingForCustomerAsync(
            this IRepository<MeterReading> repository,
            int customerId)
        {
            var readings = await repository.FindAsync(x => x.CustomerId == customerId);
            return readings.OrderByDescending(x => x.ReadingDate).FirstOrDefault();
        }

        public static async Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(
            this IRepository<MeterReading> repository,
            int customerId)
        {
            var readings = await repository.FindAsync(x => x.CustomerId == customerId);
            return readings.AsQueryable().OrderByDescending(x => x.ReadingDate);
        }
    }
} 