using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string sortBy = null);

        Task<int> GetTotalCustomersCountAsync(
            string searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null);

        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> DeactivateCustomerAsync(int id);
        Task<bool> ActivateCustomerAsync(int id);
        Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null);
        Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode);
    }
} 