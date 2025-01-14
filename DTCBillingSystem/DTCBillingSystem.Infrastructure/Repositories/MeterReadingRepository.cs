using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsByCustomerAsync(string customerId)
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .OrderBy(r => r.CustomerId)
                .ThenBy(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<MeterReading> GetLatestReadingByCustomerAsync(string customerId)
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<MeterReading> GetPreviousReadingByCustomerAsync(string customerId)
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .Skip(1)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalConsumptionByCustomerAsync(string customerId, DateTime startDate, DateTime endDate)
        {
            var readings = await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.CustomerId == customerId &&
                           r.ReadingDate >= startDate &&
                           r.ReadingDate <= endDate)
                .OrderBy(r => r.ReadingDate)
                .ToListAsync();

            if (!readings.Any())
                return 0;

            var firstReading = readings.First().Reading;
            var lastReading = readings.Last().Reading;

            return lastReading - firstReading;
        }

        public async Task<IEnumerable<MeterReading>> GetUnbilledReadingsAsync()
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => !r.IsBilled)
                .OrderBy(r => r.CustomerId)
                .ThenBy(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeterReading>> GetEstimatedReadingsAsync()
        {
            return await GetQueryableWithIncludes(r => r.Customer)
                .Where(r => r.IsEstimated)
                .OrderBy(r => r.CustomerId)
                .ThenBy(r => r.ReadingDate)
                .ToListAsync();
        }

        public async Task<bool> ValidateReadingAsync(MeterReading reading)
        {
            var previousReading = await GetPreviousReadingByCustomerAsync(reading.CustomerId);
            if (previousReading == null)
                return true;

            // Basic validation: new reading should be greater than previous reading
            return reading.Reading > previousReading.Reading;
        }

        public async Task<MeterReading> AddReadingAsync(MeterReading reading)
        {
            if (!await ValidateReadingAsync(reading))
                throw new InvalidOperationException("Invalid meter reading value");

            return await AddAsync(reading);
        }

        public async Task<MeterReading> UpdateReadingAsync(MeterReading reading)
        {
            if (!await ValidateReadingAsync(reading))
                throw new InvalidOperationException("Invalid meter reading value");

            return await UpdateAsync(reading);
        }

        public async Task<bool> DeleteReadingAsync(string readingId)
        {
            return await DeleteAsync(readingId);
        }
    }
} 