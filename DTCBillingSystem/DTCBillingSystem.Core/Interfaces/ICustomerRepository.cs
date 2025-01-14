using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByMeterNumberAsync(string meterNumber);
        Task<IEnumerable<Customer>> GetByNameAsync(string name);
        Task<bool> IsMeterNumberUniqueAsync(string meterNumber);
    }
} 