using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMeterReadingRepository : IRepository<MeterReading>
    {
        Task<MeterReading?> GetLatestReadingForCustomerAsync(int customerId);
        Task<MeterReading?> GetPreviousReadingForCustomerAsync(int customerId);
        Task<IEnumerable<MeterReading>> GetReadingsForCustomerAsync(int customerId);
        new Task UpdateAsync(MeterReading reading);
        new Task RemoveAsync(MeterReading reading);
    }
} 