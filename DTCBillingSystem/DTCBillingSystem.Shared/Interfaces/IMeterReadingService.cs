using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IMeterReadingService
    {
        Task<MeterReading> AddReadingAsync(int customerId, decimal reading, int userId, string notes = null);
        Task<IEnumerable<MeterReading>> GetReadingsForCustomerAsync(int customerId);
        Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId);
        Task<MeterReading> UpdateReadingAsync(int readingId, decimal reading, int userId, string notes = null);
        Task DeleteReadingAsync(int readingId, int userId);
    }
} 