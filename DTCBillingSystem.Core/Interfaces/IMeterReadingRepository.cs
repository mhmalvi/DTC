using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IMeterReadingRepository : IRepository<MeterReading>
    {
        Task<MeterReading?> GetLatestReadingForCustomerAsync(int customerId);
        Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId);
        new Task UpdateAsync(MeterReading reading);
        new Task RemoveAsync(MeterReading reading);
    }
} 