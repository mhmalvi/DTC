using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMeterReadingScheduleRepository : IRepository<MeterReadingSchedule>
    {
        Task<MeterReadingSchedule?> GetScheduleByDateAsync(DateTime date);
        Task<IEnumerable<MeterReadingSchedule>> GetSchedulesByZoneAsync(string zoneCode);
        Task<IEnumerable<MeterReadingSchedule>> GetSchedulesForMonthAsync(DateTime month);
        Task<bool> HasScheduleForDateAsync(DateTime date, string zoneCode);
    }
} 