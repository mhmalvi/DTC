using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class MeterReadingScheduleRepository : BaseRepository<MeterReadingSchedule>, IMeterReadingScheduleRepository
    {
        public MeterReadingScheduleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MeterReadingSchedule?> GetScheduleByDateAsync(DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.ReadingDate.Date == date.Date);
        }

        public async Task<IEnumerable<MeterReadingSchedule>> GetSchedulesByZoneAsync(string zone)
        {
            return await _dbSet
                .Where(s => s.Zone == zone)
                .OrderByDescending(s => s.ReadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeterReadingSchedule>> GetSchedulesForMonthAsync(DateTime month)
        {
            return await _dbSet
                .Where(s => s.ReadingDate.Year == month.Year && s.ReadingDate.Month == month.Month)
                .OrderBy(s => s.ReadingDate)
                .ToListAsync();
        }

        public async Task<bool> HasScheduleForDateAsync(DateTime date, string zone)
        {
            return await _dbSet
                .AnyAsync(s => s.ReadingDate.Date == date.Date && s.Zone == zone);
        }

        public async Task<IEnumerable<MeterReadingSchedule>> GetPendingSchedulesAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(s => !s.IsCompleted && s.ReadingDate.Date <= today)
                .OrderBy(s => s.ReadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeterReadingSchedule>> GetSchedulesByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.ReadingDate)
                .ToListAsync();
        }

        public async Task<MeterReadingSchedule?> GetNextScheduleForCustomerAsync(int customerId)
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(s => s.CustomerId == customerId && !s.IsCompleted && s.ReadingDate.Date >= today)
                .OrderBy(s => s.ReadingDate)
                .FirstOrDefaultAsync();
        }
    }
} 