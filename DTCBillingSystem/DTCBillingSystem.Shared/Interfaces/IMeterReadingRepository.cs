using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IMeterReadingRepository : IRepository<MeterReading>
    {
        Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId);
        Task<MeterReading> GetPreviousReadingForCustomerAsync(int customerId, DateTime currentReadingDate);
    }
} 