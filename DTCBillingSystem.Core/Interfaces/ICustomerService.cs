using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id, int userId);
        Task<bool> DeactivateCustomerAsync(int id, int userId);
        Task<bool> ActivateCustomerAsync(int id, int userId);
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> GetInactiveCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCustomersCountAsync();
    }
} 