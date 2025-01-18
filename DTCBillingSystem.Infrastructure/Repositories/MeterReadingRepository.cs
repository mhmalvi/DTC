using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MeterReading?> GetLatestReadingForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<MeterReading?> GetPreviousReadingForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .Skip(1)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReadingDate)
                .ToListAsync();
        }

        public new async Task UpdateAsync(MeterReading reading)
        {
            _dbSet.Update(reading);
            await Task.CompletedTask;
        }

        public new async Task RemoveAsync(MeterReading reading)
        {
            _dbSet.Remove(reading);
            await Task.CompletedTask;
        }
    }
} 