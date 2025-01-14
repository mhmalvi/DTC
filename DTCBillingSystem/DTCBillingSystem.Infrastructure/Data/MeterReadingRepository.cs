using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class MeterReadingRepository : BaseRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(DbContext context) : base(context)
        {
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public async Task<MeterReading> GetPreviousReadingForCustomerAsync(int customerId, DateTime currentReadingDate)
        {
            return await _dbSet
                .Where(m => m.CustomerId == customerId && m.ReadingDate < currentReadingDate)
                .OrderByDescending(m => m.ReadingDate)
                .FirstOrDefaultAsync();
        }
    }
} 