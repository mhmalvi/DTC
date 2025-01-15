using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByMeterNumberAsync(string meterNumber);
        Task<IEnumerable<Customer>> GetByNameAsync(string name);
        Task<bool> IsMeterNumberUniqueAsync(string meterNumber);
        Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string? sortBy = null);
        Task<int> GetTotalCountAsync(
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null);
        Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null);
        Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode);
        Task DeleteAsync(int id);
        Task<bool> IsShopNoUniqueAsync(string shopNo, int? excludeCustomerId = null);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchText);
    }
} 