using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMeterReadingRepository : IRepository<MeterReading>
    {
        Task<MeterReading?> GetLatestReadingAsync(int customerId);
        Task<IEnumerable<MeterReading>> GetReadingsForPeriodAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<bool> HasReadingForDateAsync(int customerId, DateTime date);
    }
} 