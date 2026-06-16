using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IMeterReadingService
    {
        Task<MeterReading> CreateReadingAsync(MeterReading reading, int userId);
        Task<MeterReading> UpdateReadingAsync(int readingId, MeterReading reading, int userId);
        Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId);
        Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId);
        Task DeleteReadingAsync(int readingId, int userId);
    }
} 